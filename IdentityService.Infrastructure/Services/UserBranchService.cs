using System.Net;
using IdentityService.Core.Entities;
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
        try
        {
            var pagedResult = await _userBranchRepository.GetPagedByStoreAsync(query, storeId);
            var response = pagedResult.Items.Select(UserBranchMapper.ToResponse);
            
            Logger.Info("Berhasil mengambil data user branch");
            return Result<IEnumerable<UserBranchResponse>>.Success(response);
        }
        catch (Exception ex)
        {
            Logger.Error("Gagal mengambil data user branch", ex);
            return Result<IEnumerable<UserBranchResponse>>
                .Failure(["Gagal mengambil data user branch"], "Internal Server Error", HttpStatusCode.InternalServerError);
        }
    }
    #endregion

    #region GetByIdAsync
    public async Task<Result<UserBranchResponse>> GetByIdAsync(Guid id)
    {
        try
        {
            var userBranch = await _userBranchRepository.GetByIdAsync(id);
            return userBranch == null ? Result<UserBranchResponse>
                .Failure(["User branch tidak ditemukan"]) : Result<UserBranchResponse>.Success(UserBranchMapper.ToResponse(userBranch));
        }
        catch (Exception ex)
        {
            Logger.Error("Gagal mengambil data user branch", ex);
            return Result<UserBranchResponse>
                .Failure(["Gagal mengambil data user branch"], "Internal Server Error", HttpStatusCode.InternalServerError);
        }
    }
    #endregion

    #region GetByUserIdAsync
    public async Task<Result<IEnumerable<UserBranchResponse>>> GetByUserIdAsync(Guid userId)
    {
        try
        {
            var userBranches = await _userBranchRepository.GetByUserIdAsync(userId);
            var resp = userBranches.Select(UserBranchMapper.ToResponse);
            
            Logger.Info("Berhasil mengambil data user branch");
            return Result<IEnumerable<UserBranchResponse>>.Success(resp);
        }
        catch (Exception ex)
        {
           Logger.Error("Gagal mengambil data user branch", ex);
              return Result<IEnumerable<UserBranchResponse>>
                .Failure(["Gagal mengambil data user branch"], "Internal Server Error", HttpStatusCode.InternalServerError);
        }
    }
    #endregion

    #region GetByBranchIdAsync
    public async Task<Result<IEnumerable<UserBranchResponse>>> GetByBranchIdAsync(Guid branchId)
    {
        try
        {
            var userBranches = await _userBranchRepository.GetByBranchIdAsync(branchId);
            var resp = userBranches.Select(UserBranchMapper.ToResponse);
            
            Logger.Info("Berhasil mengambil user branch berdasarkan branch id");
            return Result<IEnumerable<UserBranchResponse>>.Success(resp);
        }
        catch (Exception ex)
        {
            Logger.Error("Gagal mengambil user branch", ex);
            return Result<IEnumerable<UserBranchResponse>>
                .Failure(["Gagal mengambil user branch"], "Internal Server Error", HttpStatusCode.InternalServerError);
        }
    }
    #endregion

    #region CreateAsync
    public async Task<Result<UserBranchResponse>> CreateAsync(UserBranchRequest req)
    {
        try
        {
            List<string> errors = [];
            
            var branch = await _branchRepository.GetByIdAsync(req.BranchId);
            if (branch == null) errors.Add("Branch tidak ditemukan.");
            
            var existingUserBranch = await _userBranchRepository.GetByUserAndBranchAsync(req.UserId, req.BranchId);
            if (existingUserBranch != null) errors.Add("User branch sudah ditambahkan.");
            
            if (errors.Count > 0)
                return Result<UserBranchResponse>.Failure(errors);

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
        catch (Exception ex)
        {
            Logger.Error("Gagal menambah data user branch", ex);
            return Result<UserBranchResponse>
                .Failure(["Gagal menambah data user branch"], "Internal Server Error", HttpStatusCode.InternalServerError);
        }
    }
    #endregion

    #region DeleteAsync
    public async Task<Result<string>> DeleteAsync(Guid id)
    {
        try
        {
            var userBranch = await _userBranchRepository.GetByIdAsync(id);
            if (userBranch == null)
                return Result<string>.Failure(["User branch tidak ditemukan"]);

            _userBranchRepository.Delete(userBranch);
            await _unitOfWork.SaveChangesAsync();
            
            Logger.Info("Berhasil mengapus data user branch");
            return Result<string>.Success("User branch deleted successfully");
        }
        catch (Exception ex)
        {
            Logger.Error("Gagal menghapus data user branch", ex);
            return Result<string>
                .Failure(["Gagal menghapus data user branch"], "Internal Server Error", HttpStatusCode.InternalServerError);
        }
    }
    #endregion

    #region GetRowsForLookupAsync
    public async Task<Result<IEnumerable<UserBranchLookupResponse>>> GetRowsForLookupAsync(Guid storeId)
    {
        try
        {
            var userBranches = await _userBranchRepository.GetRowsForLookupAsync(storeId);
            var response = userBranches.Select(UserBranchMapper.ToLookupResponse);
            
            Logger.Info("Berhasil menambah data user branch");
            return Result<IEnumerable<UserBranchLookupResponse>>.Success(response);
        }
        catch (Exception ex)
        {
            Logger.Error("Gagal mengambil data user branch untuk lookup", ex);
            return Result<IEnumerable<UserBranchLookupResponse>>
                .Failure(["Gagal mengambil data user branch untuk lookup"], "Internal Server Error", HttpStatusCode.InternalServerError);
        }
    }
    #endregion
}
