using System.Net;
using IdentityService.Core.Entities;
using IdentityService.Core.Interfaces.Repositories;
using IdentityService.Shared.DTOs.Response;
using IdentityService.Shared.Response;
using IdentityService.Core.Interfaces.Services;
using IdentityService.Core.Mappers;
using ZivraFramework.Core.Interfaces;
using ZivraFramework.Core.Utils;
using System.Security.Cryptography;
using IdentityService.Core.Entities.Message;
using IdentityService.Core.Interfaces.Services.Message;
using IdentityService.Core.Mappers.Auth;
using IdentityService.Shared.Constants;
using IdentityService.Shared.DTOs.Request.Auth;
using IdentityService.Shared.DTOs.Request.User;
using IdentityService.Shared.DTOs.Response.Auth;

namespace IdentityService.Core.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IUserRoleRepository _userRoleRepository;
    private readonly ITokenService _tokenService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUserTokenRepository _userTokenRepository;
    private readonly IUserSecurityLogRepository _userSecurityLogRepository;
    private readonly IPasswordHistoryRepository _passwordHistoryRepository;
    private readonly IUserRegisteredEvent _userRegisteredEvent;
    private readonly IEmailVerificationEvent _emailVerificationEvent;
    private readonly IFileHelper _fileHelper;
    
    public AuthService(IUserRepository userRepository, IRoleRepository roleRepository, IUserRoleRepository userRoleRepository, ITokenService tokenService, IUnitOfWork unitOfWork, IRefreshTokenService refreshTokenService, IRefreshTokenRepository refreshTokenRepository, IUserTokenRepository userTokenRepository, IFileHelper fileHelper, IUserSecurityLogRepository userSecurityLogRepository, IPasswordHistoryRepository passwordHistoryRepository, IUserRegisteredEvent userRegisteredEvent, IEmailVerificationEvent emailVerificationEvent)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _userRoleRepository = userRoleRepository;
        _tokenService = tokenService;
        _unitOfWork = unitOfWork;
        _refreshTokenService = refreshTokenService;
        _refreshTokenRepository = refreshTokenRepository;
        _userTokenRepository = userTokenRepository;
        _fileHelper = fileHelper;
        _userSecurityLogRepository = userSecurityLogRepository;
        _passwordHistoryRepository = passwordHistoryRepository;
        _userRegisteredEvent = userRegisteredEvent;
        _emailVerificationEvent = emailVerificationEvent;
    }

    #region RegisterAsync
    public async Task<Result<AuthResponse>> RegisterAsync(RegisterRequest req)
    {
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            List<string> errors = new();
            if (await _userRepository.ExistsUsernameAsync(req.Username))
            {
                Logger.Error($"Username {req.Username} telah digunakan.");
                errors.Add("Username telah digunakan.");
            }

            if (await _userRepository.ExistsByEmailAsync(req.Email))
            {
                Logger.Error($"Email {req.Email} telah digunakan.");
                errors.Add("Email telah digunakan.");
            }

            if (errors.Count > 0)
            {
                return Result<AuthResponse>.Failure(errors, "Registrasi tidak berhasil.");
            }
            
            var user = new User
            {
                Id = Guid.NewGuid(),
                FullName = req.FullName,
                Username = req.Username,
                NormalizedUsername = req.Username.ToUpperInvariant(),
                Email = req.Email,
                NormalizedEmail = req.Email.ToUpperInvariant(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
                CreDate = DateTime.UtcNow,
                CreBy = req.CreBy,
                CreIpAddress = req.CreIpAddress,
                Address = req.Address,
                Province = req.Province,
                City = req.City,
                District = req.District,
                Rt = req.Rt,
                Rw = req.Rw
            };

            await _userRepository.AddAsync(user);

            var ownerRole = await _roleRepository.GetByNameAsync(RoleNames.Owner.ToUpper());
            if (ownerRole != null)
            {
                var userRole = new UserRole
                {
                    UserId = user.Id,
                    RoleId = ownerRole.Id,
                    CreDate = DateTime.UtcNow,
                    CreBy = req.CreBy,
                    CreIpAddress = req.CreIpAddress
                };
                await _userRoleRepository.AddAsync(userRole);
            }
            
            var securityLog = new UserSecurityLog
            {
                UserId = user.Id,
                Action = UserSecurityActions.AccountCreated,
                Description = "Pendaftaran akun berhasil",
                IpAddress = req.CreIpAddress,
                CreDate = req.CreDate,
                CreBy = req.CreBy,
                CreIpAddress = req.CreIpAddress
            };
            
            await _userSecurityLogRepository.AddAsync(securityLog);
            
            var passwordHistory = new PasswordHistory
            {
                UserId = user.Id,
                PasswordHash = user.PasswordHash,
                CreDate = req.CreDate,
                CreBy = req.CreBy,
                CreIpAddress = req.CreIpAddress
            };
            
            await _passwordHistoryRepository.AddAsync(passwordHistory);

            var roleNames = user.UserRoles
                .Select(ur => ur.Role?.Name)
                .Where(rn => !string.IsNullOrWhiteSpace(rn))
                .Select(rn => rn!)
                .ToArray();
            
            var resp = AuthMapper.ToAuthResponse(user, roleNames, null, null);
            
            var verifyToken = await SendVerifyEmailAsync(new SendVerifyEmailRequest
            {
                UserId = user.Id,
                FullName = req.FullName,
                Email = user.Email,
                Username = user.Username,
                IsSend = false,
                CreDate = DateTime.UtcNow,
                CreBy = req.CreBy,
                CreIpAddress = req.CreIpAddress,
            }, false);

            await _userRegisteredEvent.PublishAsync(new UserRegisteredEventMessage
            {
                UserId = user.Id,
                FullName = req.FullName,
                Email = user.Email,
                Username = user.Username,
                Token = verifyToken,
            });
            
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();
            
            Logger.Info($"User {user.Username} telah berhasil dibuat.");
            return Result<AuthResponse>.Success(resp);
        }
        catch (Exception e)
        {
            Logger.Error("Gagal melakukan registrasi user.", e);
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }
    #endregion

    #region RegisterStaffAsync
    public async Task<Result<RegisterStaffResponse>> RegisterStaffAsync(RegisterStaffRequest req)
    {
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            List<string> errors = new();
            if (await _userRepository.ExistsUsernameAsync(req.Username))
            {
                Logger.Error($"Username '{req.Username}' telah digunakan.");
                errors.Add("Username telah digunakan.");
            }
            
            if (await _userRepository.ExistsByEmailAsync(req.Email))
            {
                Logger.Error($"Email '{req.Email}' telah digunakan.");
                errors.Add("Email telah digunakan.");
            }
            
            if (errors.Count > 0)
            {
                return Result<RegisterStaffResponse>.Failure(errors, "Pendaftaran staff gagal.");
            }
            
            var imageUrl = await _fileHelper.SaveAsync(req.ProfileImage, "profiles");
            
            var user = new User
            {
                Id = Guid.NewGuid(),
                FullName = req.FullName,
                Username = req.Username,
                NormalizedUsername = req.Username.ToUpperInvariant(),
                Email = req.Email,
                NormalizedEmail = req.Email.ToUpperInvariant(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
                DisplayName = req.DisplayName,
                PhoneNumber = req.PhoneNumber,
                IsActive = req.IsActive,
                ProfileUrl = imageUrl,
                StoreId = req.StoreId,
                OwnerId = req.OwnerId,
                Address = req.Address,
                Province = req.Province,
                City = req.City,
                District = req.District,
                Rt = req.Rt,
                Rw = req.Rw,
                CreDate = req.CreDate,
                CreBy = req.CreBy,
                CreIpAddress = req.CreIpAddress
            };
            
            await _userRepository.AddAsync(user);
            
            var distinctRoleIds = req.RoleIDs?.Distinct() ?? Array.Empty<Guid>();
            foreach (var roleId in distinctRoleIds)
            {
                var role = await _roleRepository.GetByIdAsync(roleId);
                if (role == null) continue;
                var userRole = new UserRole
                {
                    UserId = user.Id,
                    RoleId = role.Id,
                    CreDate = req.CreDate,
                    CreBy = req.CreBy,
                    CreIpAddress = req.CreIpAddress
                };
                await _userRoleRepository.AddAsync(userRole);
            }
            
            var securityLog = new UserSecurityLog
            {
                UserId = user.Id,
                Action = UserSecurityActions.AccountCreated,
                Description = "Pendaftaran akun berhasil",
                IpAddress = req.CreIpAddress,
                CreDate = req.CreDate,
                CreBy = req.CreBy,
                CreIpAddress = req.CreIpAddress
            };
            
            await _userSecurityLogRepository.AddAsync(securityLog);
            
            var passwordHistory = new PasswordHistory
            {
                UserId = user.Id,
                PasswordHash = user.PasswordHash,
                CreDate = req.CreDate,
                CreBy = req.CreBy,
                CreIpAddress = req.CreIpAddress
            };
            
            await _passwordHistoryRepository.AddAsync(passwordHistory);
            
            var verifyToken = await SendVerifyEmailAsync(new SendVerifyEmailRequest
            {
                UserId = user.Id,
                FullName = req.FullName,
                Email = user.Email,
                Username = user.Username,
                IsSend = false,
                CreDate = DateTime.UtcNow,
                CreBy = req.CreBy,
                CreIpAddress = req.CreIpAddress,
            }, false);

            await _userRegisteredEvent.PublishAsync(new UserRegisteredEventMessage
            {
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Username = user.Username,
                Token = verifyToken,
            });
            
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();
            
            var resp = RegisterStaffMapper.ToRegisterStaffResponse(user);
            Logger.Info($"Staff dengan Username '{user.Username}' berhasil didaftarkan.");
            return Result<RegisterStaffResponse>.Success(resp);
        }
        catch (Exception e)
        {
            Logger.Error("Terjadi kesalahan pada server saat mendaftarkan staff.", e);
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }
    #endregion

    #region UpdateUserAsync
    public async Task<Result<UpdateUserResponse>> UpdateUserAsync(UpdateUserRequest req)
    {
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var user = await _userRepository.GetByIdAsync(req.Id);
            if (user == null)
            {
                Logger.Error($"User dengan Id '{req.Id}' tidak ditemukan.");
                return Result<UpdateUserResponse>.Failure(new List<string> { "User tidak ditemukan." }, "Update user gagal.");
            }
            
            var imageUrl = await _fileHelper.ReplaceIfChangedAsync(req.ProfilImage, user.ProfileUrl, "profiles");
            user.DisplayName = req.DisplayName;
            user.PhoneNumber = req.PhoneNumber;
            user.IsActive = req.IsActive ?? user.IsActive;
            user.ProfileUrl = imageUrl;
            user.Address = req.Address;
            user.Province = req.Province;
            user.City = req.City;
            user.District = req.District;
            user.Rt = req.Rt;
            user.Rw = req.Rw;
            user.ModDate = req.ModDate;
            user.ModBy = req.ModBy;
            user.ModIpAddress = req.ModIpAddress;
            
            _userRepository.Update(user);
            
            var securityLog = new UserSecurityLog
            {
                UserId = user.Id,
                Action = UserSecurityActions.AccountUpdated,
                Description = "Akun diperbarui",
                IpAddress = req.ModIpAddress,
                CreDate = req.ModDate ?? DateTime.UtcNow,
                CreBy = req.ModBy,
                CreIpAddress = req.ModIpAddress
            };
            
            await _userSecurityLogRepository.AddAsync(securityLog);
            
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();
            
            var resp = UpdateUserMapper.ToUpdateUserResponse(user);
            Logger.Info($"User dengan Id '{user.Id}' berhasil diperbarui.");
            return Result<UpdateUserResponse>.Success(resp);
        }
        catch (Exception e)
        {
            Logger.Error("Terjadi kesalahan saat memperbarui user.", e);
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }
    #endregion

    #region LoginAsync
    public async Task<Result<AuthResponse>> LoginAsync(LoginRequest req)
    {
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            List<string> errors = new();
            
            var user = await _userRepository.GetByUsernameAsync(req.Username);
            if (user is null)
                errors.Add("Username tidak ditemukan.");
            
            if (user is not null && !BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
                errors.Add("Password salah.");
            
            if (errors.Count > 0)
                return Result<AuthResponse>.Failure(errors);

            if (user is { EmailConfirmed: false })
            {
                return Result<AuthResponse>.Failure(["Email belum terverifikasi."], "Login gagal.", HttpStatusCode.Forbidden);
            }

            var userRoles = await _userRoleRepository.GetRowsByUserIdAsync(user!.Id);
            var roleNames = userRoles
                .Select(ur => ur.Role?.Name)
                .Where(rn => !string.IsNullOrWhiteSpace(rn))
                .Select(rn => rn!)
                .ToArray();
            
            var securityLog = new UserSecurityLog
            {
                UserId = user.Id,
                Action = UserSecurityActions.AccountUpdated,
                Description = "User login berhasil",
                IpAddress = req.ModIpAddress,
                CreDate = req.ModDate ?? DateTime.UtcNow,
                CreBy = req.ModBy,
                CreIpAddress = req.ModIpAddress
            };
            
            await _userSecurityLogRepository.AddAsync(securityLog);
            
            await _unitOfWork.SaveChangesAsync();

            var token = _tokenService.GenerateJwtToken(user, roleNames);
            var refreshToken = await _refreshTokenService.GenerateAndStoreRefreshTokenAsync(user.Id);

            var resp = AuthMapper.ToAuthResponse(user, roleNames, token, refreshToken);

            await _unitOfWork.CommitTransactionAsync();
            Logger.Info($"User '{user.Username}' berhasil login.");

            return Result<AuthResponse>.Success(resp);
        }
        catch (Exception e)
        {
            Logger.Error("Proses login gagal.", e);
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }
    #endregion

    #region LogoutAsync
    public async Task<Result<string>> LogoutAsync(Guid userId)
    {
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var tokens = await _refreshTokenRepository.GetActiveByUserIdAsync(userId);
            foreach (var t in tokens)
            {
                await _refreshTokenService.RevokeRefreshTokenAsync(t.Id);
            }
            
            var securityLog = new UserSecurityLog
            {
                UserId = userId,
                Action = UserSecurityActions.Logout,
                Description = "User logout",
            };
            
            await _userSecurityLogRepository.AddAsync(securityLog);

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            Logger.Info($"User dengan Id '{userId}' berhasil logout.");
            return Result<string>.Success("Logout berhasil.");
        }
        catch (Exception e)
        {
            Logger.Error("Logout gagal.", e);
            await _unitOfWork.RollbackTransactionAsync();
            return Result<string>.Failure(new List<string> { "Terjadi kesalahan saat logout." }, "Logout gagal.");
        }
    }
    #endregion

    #region RequestPasswordResetAsync
    public async Task<Result<ForgotPasswordResponse>> RequestPasswordResetAsync(ForgotPasswordRequest req)
    {
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var user = await _userRepository.GetByEmailAsync(req.Email);
            if (user == null)
                return Result<ForgotPasswordResponse>.Failure(new List<string> { "Email tidak terdaftar." }, "Request reset gagal.");

            var rawToken = GenerateRandomToken();
            var hashedToken = HashToken(rawToken);

            var userToken = new UserToken
            {
                Id = req.Id,
                UserId = user.Id,
                LoginProvider = "Local",
                Name = "PasswordReset",
                Value = hashedToken,
                CreDate = req.CreDate,
                CreBy = req.CreBy,
                CreIpAddress = req.CreIpAddress
            };

            await _userTokenRepository.AddAsync(userToken);
            
            var securityLog = new UserSecurityLog
            {
                UserId = user.Id,
                Action = UserSecurityActions.PasswordResetRequested,
                Description = "Permintaan reset password dibuat",
                IpAddress = req.ModIpAddress,
                CreDate = req.ModDate ?? DateTime.UtcNow,
                CreBy = req.ModBy,
                CreIpAddress = req.ModIpAddress
            };
            
            await _userSecurityLogRepository.AddAsync(securityLog);
            
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();
            
            userToken.Value = rawToken;
            var resp = ForgotPasswordMapper.ToForgotPasswordResponse(userToken);
            return Result<ForgotPasswordResponse>.Success(resp);
        }
        catch (Exception e)
        {
            await _unitOfWork.RollbackTransactionAsync();
            Logger.Error("Gagal membuat req password reset.", e);
            throw;
        }
    }
    #endregion

    #region ResetPasswordAsync
    public async Task<Result<string>> ResetPasswordAsync(ResetPasswordRequest req)
    {
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var hashToken = HashToken(req.Token);
            var userToken = await _userTokenRepository.GetByNameAndValueAsync("PasswordReset", hashToken);
            if (userToken == null)
                return Result<string>.Failure(["Token reset tidak valid."], "Reset gagal.");

            var expiryHours = 1;
            if (userToken.CreDate.AddHours(expiryHours) < DateTime.UtcNow)
            {
                await _userTokenRepository.DeleteAsync(userToken);
                return Result<string>.Failure(["Token reset telah kedaluwarsa."], "Reset gagal.");
            }

            var user = await _userRepository.GetByIdAsync(userToken.UserId);
            if (user == null)
            {
                await _userTokenRepository.DeleteAsync(userToken);
                return Result<string>.Failure(["User tidak ditemukan."], "Reset gagal.");
            }
            
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.NewPassword);
            user.ModDate = DateTime.UtcNow;
            user.ModBy = req.ModBy;
            user.ModIpAddress = req.ModIpAddress;
            
            var passwordHistory = await _passwordHistoryRepository.GetRowByPasswordHashAsync(user.PasswordHash);
            if (passwordHistory is not null)
            {
                Logger.Error("Password telah digunakan sebelumnya.");
                return Result<string>.Failure(["Password telah digunakan sebelumnya."], "Reset gagal.");
            }

            await _userTokenRepository.DeleteAsync(userToken);

            var tokens = await _refreshTokenRepository.GetActiveByUserIdAsync(user.Id);
            foreach (var t in tokens)
            {
                await _refreshTokenService.RevokeRefreshTokenAsync(t.Id);
            }
            
            var securityLog = new UserSecurityLog
            {
                UserId = user.Id,
                Action = UserSecurityActions.PasswordChanged,
                Description = "Password berhasil direset",
                IpAddress = req.ModIpAddress,
                CreDate = req.ModDate ?? DateTime.UtcNow,
                CreBy = req.ModBy,
                CreIpAddress = req.ModIpAddress
            };
            
            await _userSecurityLogRepository.AddAsync(securityLog);

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            return Result<string>.Success("Password berhasil direset.");
        }
        catch (Exception e)
        {
            await _unitOfWork.RollbackTransactionAsync();
            Logger.Error("Gagal mereset password.", e);
            throw;
        }
    }
    #endregion
    
    #region SendVerifyEmailAsync
    public async Task<string> SendVerifyEmailAsync(SendVerifyEmailRequest req, bool withTxn = true)
    {
        if (withTxn) await _unitOfWork.BeginTransactionAsync();
        try
        {
            var rawToken = GenerateRandomToken();
            var hashedToken = HashToken(rawToken);
            
            var user = await _userRepository.GetByIdAsync(req.UserId);
            if (user == null)
                throw new Exception($"User tidak ditemukan");
            
            var existingToken = await _userTokenRepository.ExistByNameAndUserIdAsync("EmailVerification", req.UserId);
            if (existingToken)
            {
                await _userTokenRepository.DeleteByNameAndUserIdAsync("EmailVerification", req.UserId);
            }
            
            var userToken = new UserToken()
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                LoginProvider = "Local",
                Name = "EmailVerification",
                Value = hashedToken,
                CreDate = req.CreDate,
                CreBy = req.CreBy,
                CreIpAddress = req.CreIpAddress
            };
            
            await _userTokenRepository.AddAsync(userToken);
            
            var userSecurityLog = new UserSecurityLog
            {
                Id = Guid.NewGuid(),
                UserId = req.UserId,
                Action = UserSecurityActions.SendEmailVerification,
                Description = "Email verifikasi dikirim",
                IpAddress = req.CreIpAddress,
                CreDate = req.CreDate,
                CreBy = req.CreBy,
                CreIpAddress = req.CreIpAddress
            };
            
            await _userSecurityLogRepository.AddAsync(userSecurityLog);
            
            if (withTxn)
            {
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();
            }

            if (req.IsSend)
            {
                var emailMessage = new EmailVerificationEventMessage
                {
                    FullName = user.FullName ?? string.Empty,
                    Email = user.Email,
                    Username = user.Username,
                    Token = rawToken
                };
                
                await _emailVerificationEvent.PublishAsync(emailMessage);
            }
            
            Logger.Info($"Email verifikasi berhasil dikirim ke user dengan Id '{req.UserId}'.");
            return rawToken;
        }
        catch (Exception e)
        {
            if (withTxn) await _unitOfWork.RollbackTransactionAsync();
            Logger.Error("Gagal mengirim email verifikasi.", e);
            throw;
        }
    }
    #endregion
    
    #region VerifyEmailAsync
    public async Task<Result<string>> VerifyEmailAsync(string token)
    {
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var hashToken = HashToken(token);
            var userToken = await _userTokenRepository.GetByNameAndValueAsync("EmailVerification", hashToken);
            if (userToken == null)
                return Result<string>.Failure(new List<string> { "Token verifikasi tidak valid." }, "Verifikasi gagal.");
            
            var expiryHours = 24;
            if (userToken.CreDate.AddHours(expiryHours) < DateTime.UtcNow)
            {                
                await _userTokenRepository.DeleteAsync(userToken);
                return Result<string>.Failure(new List<string> { "Token verifikasi telah kedaluwarsa." }, "Verifikasi gagal.");
            }
            
            var user = await _userRepository.GetByIdAsync(userToken.UserId);
            if (user == null)
            {
                await _userTokenRepository.DeleteAsync(userToken);
                return Result<string>.Failure(new List<string> { "User tidak ditemukan." }, "Verifikasi gagal.");
            }
            
            user.EmailConfirmed = true;
            user.ModDate = DateTime.UtcNow;
            user.ModBy = "System";
            user.ModIpAddress = "System";
            
            await _userTokenRepository.DeleteAsync(userToken);
            _userRepository.Update(user);
            
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();
            
            Logger.Info($"Email untuk user dengan Id '{user.Id}' telah terverifikasi.");
            return Result<string>.Success("Email berhasil diverifikasi.");
        }
        catch (Exception e)
        {
            await _unitOfWork.RollbackTransactionAsync();
            Logger.Error("Gagal memverifikasi email.", e);
            throw;
        }
    }
    #endregion

    private static string GenerateRandomToken(int size = 48)
    {
        var bytes = new byte[size];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").TrimEnd('=');
    }

    private static string HashToken(string token)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(token);
        var hash = SHA256.HashData(bytes);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}
