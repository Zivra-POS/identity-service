using IdentityService.Core.Entities;
using IdentityService.Core.Exceptions;
using IdentityService.Core.Interfaces.Repositories;
using IdentityService.Core.Interfaces.Services;
using IdentityService.Core.Mappers.UserBranch;
using IdentityService.Shared.DTOs.Request.UserBranch;
using IdentityService.Shared.DTOs.Response.UserBranch;
using IdentityService.Shared.Response;
using ZivraFramework.Core.Interfaces;
using ZivraFramework.Core.Models;
using ZivraFramework.Core.Utils;

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
    public async Task<Result<IEnumerable<UserBranchResponse>>> GetAllAsync(PagedQuery query, Guid storeId)
    {
        var pagedResult = await _userBranchRepository.GetPagedByStoreAsync(query, storeId);
        var response = pagedResult.Items.Select(UserBranchMapper.ToResponse);
        
        Logger.Info("Berhasil mengambil data user branch");
        return Result<IEnumerable<UserBranchResponse>>.Success(response);
    }
    #endregion

    #region GetByIdAsync
    public async Task<Result<UserBranchResponse>> GetByIdAsync(Guid id)
    {
        var userBranch = await _userBranchRepository.GetByIdAsync(id);
        if (userBranch == null)
            throw new NotFoundException("User branch tidak ditemukan");
            
        Logger.Info("Berhasil mengambil data user branch");
        return Result<UserBranchResponse>.Success(UserBranchMapper.ToResponse(userBranch));
    }
    #endregion

    #region GetByUserIdAsync
    public async Task<Result<IEnumerable<UserBranchResponse>>> GetByUserIdAsync(Guid userId)
    {
        var userBranches = await _userBranchRepository.GetByUserIdAsync(userId);
        var resp = userBranches.Select(UserBranchMapper.ToResponse);
        
        Logger.Info("Berhasil mengambil data user branch");
        return Result<IEnumerable<UserBranchResponse>>.Success(resp);
    }
    #endregion

    #region GetByBranchIdAsync
    public async Task<Result<IEnumerable<UserBranchResponse>>> GetByBranchIdAsync(Guid branchId)
    {
        var userBranches = await _userBranchRepository.GetByBranchIdAsync(branchId);
        var resp = userBranches.Select(UserBranchMapper.ToResponse);
        
        Logger.Info("Berhasil mengambil user branch berdasarkan branch id");
        return Result<IEnumerable<UserBranchResponse>>.Success(resp);
    }
    #endregion

    #region CreateAsync
    public async Task<Result<UserBranchResponse>> CreateAsync(UserBranchRequest req)
    {
        var branch = await _branchRepository.GetByIdAsync(req.BranchId);
        if (branch == null)
            throw new NotFoundException("Branch tidak ditemukan.");
        
        var existingUserBranch = await _userBranchRepository.GetByUserAndBranchAsync(req.UserId, req.BranchId);
        if (existingUserBranch != null)
            throw new ValidationException("User branch sudah ditambahkan.");

        var userBranch = new UserBranch
            {
                Id = Guid.NewGuid(),
                UserId = req.UserId,
                BranchId = req.BranchId,
                IsPrimary = req.IsPrimary,
                CreDate = req.CreDate,
                CreBy = req.CreBy,
                CreIpAddress = req.CreIpAddress
            };

        await _userBranchRepository.AddAsync(userBranch);
        await _unitOfWork.SaveChangesAsync();
        
        Logger.Info("Berhasil menambah data branch ke user");
        var resp = await _userBranchRepository.GetByIdAsync(userBranch.Id);
        return Result<UserBranchResponse>.Success(UserBranchMapper.ToResponse(resp!));
    }
    #endregion

    #region DeleteAsync
    public async Task<Result<string>> DeleteAsync(Guid id)
    {
        var userBranch = await _userBranchRepository.GetByIdAsync(id);
        if (userBranch == null)
            throw new NotFoundException("User branch tidak ditemukan");

        _userBranchRepository.Delete(userBranch);
        await _unitOfWork.SaveChangesAsync();
        
        Logger.Info("Berhasil mengapus data user branch");
        return Result<string>.Success("User branch deleted successfully");
    }
    #endregion

    #region GetRowsForLookupAsync
    public async Task<Result<IEnumerable<UserBranchLookupResponse>>> GetRowsForLookupAsync(Guid storeId)
    {
        var userBranches = await _userBranchRepository.GetRowsForLookupAsync(storeId);
        var response = userBranches.Select(UserBranchMapper.ToLookupResponse);
        
        Logger.Info("Berhasil mengambil data user branch untuk lookup");
        return Result<IEnumerable<UserBranchLookupResponse>>.Success(response);
    }
    #endregion
}
