using IdentityService.Core.Entities;
using IdentityService.Core.Exceptions;
using IdentityService.Core.Interfaces.Repositories;
using IdentityService.Core.Interfaces.Services;
using IdentityService.Core.Mappers.User;
using IdentityService.Shared.DTOs.UserClaim;
using IdentityService.Shared.Response;
using ZivraFramework.Core.Interfaces;

namespace IdentityService.Infrastructure.Services;

public class UserClaimService : IUserClaimService
{
    private readonly IUserClaimRepository _ucRepo;
    private readonly IUserRepository _userRepo;
    private readonly IUnitOfWork _unitOfWork;

    public UserClaimService(IUserClaimRepository ucRepo, IUserRepository userRepo, IUnitOfWork unitOfWork)
    {
        _ucRepo = ucRepo;
        _userRepo = userRepo;
        _unitOfWork = unitOfWork;
    }

    #region GetByUserIdAsync
    public async Task<Result<IEnumerable<UserClaimResponse>>> GetByUserIdAsync(Guid userId)
    {
        var rows = await _ucRepo.GetByUserIdAsync(userId);
        var res = rows.Select(UserClaimMapper.ToResponse);
        return Result<IEnumerable<UserClaimResponse>>.Success(res);
    }
    #endregion

    #region GetByIdAsync
    public async Task<Result<UserClaimResponse>> GetByIdAsync(Guid id)
    {
        var uc = await _ucRepo.GetByIdAsync(id);
        if (uc == null)
            throw new NotFoundException("User Claim tidak ditemukan.");

        return Result<UserClaimResponse>.Success(UserClaimMapper.ToResponse(uc));
    }
    #endregion

    #region CreateAsync
    public async Task<Result<UserClaimResponse>> CreateAsync(UserClaimRequest req)
    {
        if (req.UserId == Guid.Empty)
            throw new ValidationException("User Id wajib diisi.");

        var user = await _userRepo.GetByIdAsync(req.UserId);
        if (user == null)
            throw new NotFoundException("User tidak ditemukan.");

        var uc = new UserClaim
        {
            Id = req.Id ?? Guid.NewGuid(),
            UserId = req.UserId,
            ClaimType = req.ClaimType,
            ClaimValue = req.ClaimValue,
            CreDate = DateTime.UtcNow
        };

        await _ucRepo.AddAsync(uc);
        await _unitOfWork.SaveChangesAsync();

        return Result<UserClaimResponse>.Success(UserClaimMapper.ToResponse(uc));
    }
    #endregion

    #region UpdateAsync
    public async Task<Result<UserClaimResponse>> UpdateAsync(UserClaimRequest req)
    {
        if (req.Id == null || req.Id == Guid.Empty)
            throw new ValidationException("Id wajib diisi.");

        var uc = await _ucRepo.GetByIdAsync(req.Id.Value);
        if (uc == null)
            throw new NotFoundException("User Claim tidak ditemukan.");

        uc.ClaimType = req.ClaimType;
        uc.ClaimValue = req.ClaimValue;
        uc.ModDate = DateTime.UtcNow;

        _ucRepo.Update(uc);
        await _unitOfWork.SaveChangesAsync();

        return Result<UserClaimResponse>.Success(UserClaimMapper.ToResponse(uc));
    }
    #endregion

    #region DeleteAsync
    public async Task<Result<string>> DeleteAsync(Guid id)
    {
        var uc = await _ucRepo.GetByIdAsync(id);
        if (uc == null)
            throw new NotFoundException("User Claim tidak ditemukan.");

        _ucRepo.Delete(uc);
        await _unitOfWork.SaveChangesAsync();

        return Result<string>.Success("Deleted");
    }
    #endregion
}
