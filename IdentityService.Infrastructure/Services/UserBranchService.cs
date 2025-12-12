using IdentityService.Core.Entities;
using IdentityService.Core.Interfaces.Repositories;
using IdentityService.Core.Interfaces.Services;
using IdentityService.Core.Mappers.UserBranch;
using IdentityService.Shared.DTOs.Request.UserBranch;
using IdentityService.Shared.DTOs.Response.UserBranch;
using IdentityService.Shared.Response;
using ZivraFramework.Core.API.Exception;
using ZivraFramework.Core.Interfaces;
using ZivraFramework.Core.Models;
using ZivraFramework.Core.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using ZivraFramework.Core.Filtering.Entities;

namespace IdentityService.Infrastructure.Services;

public class UserBranchService : IUserBranchService
{
    private readonly IUserBranchRepository _userBranchRepository;
    private readonly IBranchRepository _branchRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UserBranchService(
        IUserBranchRepository userBranchRepository,
        IBranchRepository branchRepository, 
        IUnitOfWork unitOfWork)
    {
        _userBranchRepository = userBranchRepository;
        _branchRepository = branchRepository;
        _unitOfWork = unitOfWork;
    }

    #region GetAllAsync
    public async Task<PagedResult<UserBranchResponse>> GetAllAsync(QueryRequest query, Guid storeId)
    {
        var pagedResult = await _userBranchRepository.GetPagedByStoreAsync(query, storeId);
        
        Logger.Info("Berhasil mengambil data user branch");
        return pagedResult;
    }
    #endregion

    #region GetByIdAsync
    public async Task<UserBranchResponse> GetByIdAsync(Guid id)
    {
        var userBranch = await _userBranchRepository.GetByIdAsync(id);
        if (userBranch == null)
            throw new NotFoundException("User branch tidak ditemukan");
            
        Logger.Info("Berhasil mengambil data user branch");
        return UserBranchMapper.ToResponse(userBranch);
    }
    #endregion

    #region GetByHashedIdAsync
    public async Task<UserBranchResponse> GetByHashedIdAsync(string hashedId)
    {
        var ub = await _userBranchRepository.GetByHashedIdAsync(hashedId);
        if (ub == null)
            throw new NotFoundException("User branch tidak ditemukan");
        return UserBranchMapper.ToResponse(ub);
    }
    #endregion

    #region GetByUserIdAsync
    public async Task<IEnumerable<UserBranchResponse>> GetByUserIdAsync(Guid userId)
    {
        var userBranches = await _userBranchRepository.GetByUserIdAsync(userId);
        var resp = userBranches.Select(UserBranchMapper.ToResponse);
        
        Logger.Info("Berhasil mengambil data user branch");
        return resp;
    }
    #endregion

    #region GetByBranchIdAsync
    public async Task<IEnumerable<UserBranchResponse>> GetByBranchIdAsync(Guid branchId)
    {
        var userBranches = await _userBranchRepository.GetByBranchIdAsync(branchId);
        var resp = userBranches.Select(UserBranchMapper.ToResponse);
        
        Logger.Info("Berhasil mengambil user branch berdasarkan branch id");
        return resp;
    }
    #endregion

    #region CreateAsync
    public async Task<UserBranchResponse> CreateAsync(UserBranchRequest req)
    {
        var branch = await _branchRepository.GetByIdAsync(req.BranchId);
        if (branch == null)
            throw new NotFoundException("Branch tidak ditemukan.");
        
        Guid userId;

        var encoded = req.UserId;
        if (encoded != null)
        {
            userId = Base62Guid.Decode(encoded, "u_");
        }
        else
        {
            throw new ValidationException("User Id tidak ditemukan.");
        }
        
        var existingUserBranch = await _userBranchRepository.GetByUserAndBranchAsync(userId, req.BranchId);
        if (existingUserBranch != null)
            throw new ValidationException("User branch sudah ditambahkan.");

        var userBranch = new UserBranch
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                BranchId = req.BranchId,
                IsPrimary = req.IsPrimary,
                CreDate = req.CreDate ?? DateTime.UtcNow,
                CreBy = req.CreBy,
                CreIpAddress = req.CreIpAddress
            };

        await _userBranchRepository.AddAsync(userBranch);
        await _unitOfWork.SaveChangesAsync();
        
        Logger.Info("Berhasil menambah data branch ke user");
        var resp = await _userBranchRepository.GetByIdAsync(userBranch.Id);
        return UserBranchMapper.ToResponse(resp!);
    }
    #endregion
    
    #region CreteBulkAsync
    public async Task<int> CreateBulkAsync(IEnumerable<UserBranchRequest> requests)
    {
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            List<string> errors = [];
            Guid userId;

            var userBranchRequests = requests.ToList();
            var encoded = userBranchRequests!.FirstOrDefault()!.UserId;
            if (encoded != null)
            {
                userId = Base62Guid.Decode(encoded, "u_");
            }
            else
            {
                throw new ValidationException("User Id tidak ditemukan.");
            }

            foreach (var req in userBranchRequests)
            {
                var branch = await _branchRepository.GetByIdAsync(req.BranchId);
                if (branch == null) errors.Add($"Branch {req.BranchName} tidak ditemukan.");
        
                var existingUserBranch = await _userBranchRepository.GetByUserAndBranchAsync(userId, req.BranchId);
                if (existingUserBranch != null)
                    errors.Add($"Branch {req.BranchName} sudah ditambahkan ke user.");

                var userBranch = new UserBranch
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    BranchId = req.BranchId,
                    IsPrimary = req.IsPrimary,
                    CreDate = req.CreDate ?? DateTime.UtcNow,
                    CreBy = req.CreBy,
                    CreIpAddress = req.CreIpAddress
                };

                await _userBranchRepository.AddAsync(userBranch);
                Logger.Info("Berhasil menambah data branch ke user");
            }
            
            if (errors.Any())
                throw new ValidationException(errors);
            
            var result = await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();
            
            return result;
        }
        catch (Exception e)
        {
            Logger.Error("Gagal menambah data user branch secara bulk", e);
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }
    #endregion

    #region DeleteAsync
    public async Task<string> DeleteAsync(Guid id)
    {
        var userBranch = await _userBranchRepository.GetByIdAsync(id);
        if (userBranch == null)
            throw new NotFoundException("User branch tidak ditemukan");

        _userBranchRepository.Delete(userBranch);
        await _unitOfWork.SaveChangesAsync();
        
        Logger.Info("Berhasil mengapus data user branch");
        return "User branch deleted successfully";
    }
    #endregion
    
    #region DeleteBulkAsync
    public async Task<int> DeleteBulkAsync(List<Guid> ids)
    {
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            foreach (var id in ids)
            {
                var b = new UserBranch()
                {
                    Id = id
                };
                
                _userBranchRepository.Delete(b);
            }
            
            var result =  await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();
            return result;
        }
        catch (Exception e)
        {
            Logger.Error("Gagal menambah data user branch secara", e);
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }
    #endregion

    #region GetRowsForLookupAsync
    public async Task<IEnumerable<UserBranchLookupResponse>> GetRowsForLookupAsync(Guid storeId)
    {
        var userBranches = await _userBranchRepository.GetRowsForLookupAsync(storeId);
        var response = userBranches.Select(UserBranchMapper.ToLookupResponse);
        
        Logger.Info("Berhasil mengambil data user branch untuk lookup");
        return response;
    }
    #endregion
    
    #region ChangePrimaryBranchAsync
    public async Task<int> ChangePrimaryBranchAsync(ChangePrimaryBranchRequest req)
    {
        var userBranch = await _userBranchRepository.GetByUserAndBranchAsync(req.UserId, req.BranchId);
        if (userBranch == null)
            throw new NotFoundException("User branch tidak ditemukan");

        if (req.IsPrimary)
        {
            await _userBranchRepository.SetFalseIsPrimaryAsync(req.UserId);

            userBranch.IsPrimary = true;
            _userBranchRepository.Update(userBranch);

            var result = await _unitOfWork.SaveChangesAsync();
            Logger.Info("Berhasil menjadikan cabang ini sebagai primary");
            return result;
        }

        if (!req.IsPrimary)
        {
            if (userBranch.IsPrimary)
            {
                throw new BusinessException("Silakan jadikan cabang lain sebagai utama terlebih dahulu.");
            }

            userBranch.IsPrimary = false;
            _userBranchRepository.Update(userBranch);

            var result = await _unitOfWork.SaveChangesAsync();
            Logger.Info("Cabang non-primary diupdate");
            return result;
        }

        return 0;
    }
    #endregion
}
