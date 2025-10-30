using System.Net;
using IdentityService.Core.Entities;
using IdentityService.Core.Interfaces.Repositories;
using IdentityService.Core.Interfaces.Services;
using IdentityService.Core.Mappers.Store;
using IdentityService.Shared.DTOs.Request.Store;
using IdentityService.Shared.DTOs.Response.Store;
using IdentityService.Shared.Response;
using ZivraFramework.Core.Models;
using ZivraFramework.Core.Interfaces;
using ZivraFramework.Core.Utils;

namespace IdentityService.Infrastructure.Services;

public class StoreService : IStoreService
{
    private readonly IStoreRepository _storeRepo;
    private readonly IUserRepository _userRepo;
    private readonly IBranchRepository _branchRepo;
    private readonly IUnitOfWork _unitOfWork;

    public StoreService(
        IStoreRepository storeRepo, 
        IUnitOfWork unitOfWork, 
        IUserRepository userRepo, 
        IBranchRepository branchRepo)
    {
        _storeRepo = storeRepo;
        _unitOfWork = unitOfWork;
        _userRepo = userRepo;
        _branchRepo = branchRepo;
    }

    #region GetAllAsync
    public async Task<Result<IEnumerable<StoreResponse>>> GetAllAsync(PagedQuery query)
    {
        var paged = await _storeRepo.GetPagedAsync(query);
        var resList = paged.Items.Select(s => StoreMapper.ToResponse(s)).ToList();
        return Result<IEnumerable<StoreResponse>>.Success(resList);
    }
    #endregion

    #region GetByIdAsync
    public async Task<Result<StoreResponse>> GetByIdAsync(Guid id)
    {
        var store = await _storeRepo.GetByIdAsync(id);
        if (store == null) return Result<StoreResponse>.Failure(["Toko tidak ditemukan."], "Not found", HttpStatusCode.NotFound);
        return Result<StoreResponse>.Success(StoreMapper.ToResponse(store));
    }
    #endregion

    #region CreateAsync
    public async Task<Result<StoreResponse>> CreateAsync(StoreRequest req)
    {
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var user = await _userRepo.GetUserWithRolesAsync(req.UserId);
            if (user == null)
                return Result<StoreResponse>.Failure(["User tidak ditemukan."], "Validation failed");
            
            if (user.RoleNames.All(ur => ur is not "OWNER"))
                return Result<StoreResponse>.Failure(["Hanya owner yang dapat membuat toko."], "Validation failed");
            
            var lastCode = await _storeRepo.GetLastCodeAsync();
            lastCode = string.IsNullOrEmpty(lastCode) ? "0000000000000000" : (long.Parse(lastCode) + 1).ToString("D16");

            var store = new Store
            {
                Id = req.Id ?? Guid.NewGuid(),
                Name = req.Name,
                Code = lastCode,
                Address = req.Address,
                Province = req.Province,
                City = req.City,
                District = req.District,
                Rt = req.Rt,
                Rw = req.Rw,
                Phone = req.Phone,
                IsActive = req.IsActive,
                CostingMethod = req.CostingMethod,
                CreDate = req.CreDate,
                CreBy = req.CreBy,
                CreIpAddress = req.CreIpAddress
            };
            
            var branch = new Branch
            {
                Id = Guid.NewGuid(),
                StoreId = store.Id,
                Name = "Utama",
                Code = "PST",
                Address = store.Address,
                Province = store.Province,
                City = store.City,
                District = store.District,
                Rt = store.Rt,
                Rw = store.Rw,
                Phone = store.Phone,
                IsActive = true,
                CreDate = DateTime.UtcNow,
                CreBy = req.CreBy,
                CreIpAddress = req.CreIpAddress
            };
            
            await _branchRepo.AddAsync(branch);
            await _userRepo.UpdateStoreIdAsync(user.Id, store.Id);
            await _storeRepo.AddAsync(store);
            await _unitOfWork.SaveChangesAsync();
            
            Logger.Info("Toko berhasil dibuat dengan Id: " + store.Id);

            return Result<StoreResponse>.Success(StoreMapper.ToResponse(store));
        }
        catch (Exception e)
        {
            Logger.Error("Gagal membuat toko", e);
            throw;
        }
    }
    #endregion

    #region UpdateAsync
    public async Task<Result<StoreResponse>> UpdateAsync(StoreRequest req)
    {
        try
        {
            var store = await _storeRepo.GetByIdAsync(req.Id!.Value);
            if (store == null)
                return Result<StoreResponse>.Failure(["Toko tidak ditemukan."], "Not found", HttpStatusCode.NotFound);

            store.Name = req.Name;
            store.Address = req.Address;
            store.Province = req.Province;
            store.City = req.City;
            store.District = req.District;
            store.Rt = req.Rt;
            store.Rw = req.Rw;
            store.Phone = req.Phone;
            store.IsActive = req.IsActive;
            store.CostingMethod = req.CostingMethod;
            store.ModDate = DateTime.UtcNow;

            _storeRepo.Update(store);
            await _unitOfWork.SaveChangesAsync();

            return Result<StoreResponse>.Success(StoreMapper.ToResponse(store));
        }
        catch (Exception e)
        {
            Logger.Error("Gagal memperbarui toko", e);
            throw;
        }
    }
    #endregion

    #region DeleteAsync
    public async Task<Result<string>> DeleteAsync(Guid id)
    {
        try
        {
            var store = await _storeRepo.GetByIdAsync(id);
            if (store == null)
                return Result<string>.Failure(["Toko tidak ditemukan."], "Not found", HttpStatusCode.NotFound);

            _storeRepo.Delete(store);
            await _unitOfWork.SaveChangesAsync();

            return Result<string>.Success("Berhasil dihapus");
        }
        catch (Exception e)
        {
            Logger.Error("Gagal menghapus toko", e);
            throw;
        }
    }
    #endregion
}
