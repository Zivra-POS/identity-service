using IdentityService.Core.Entities;
using IdentityService.Core.Interfaces.Repositories;
using IdentityService.Core.Interfaces.Services;
using IdentityService.Core.Mappers.Store;
using IdentityService.Shared.DTOs.Request.Store;
using IdentityService.Shared.DTOs.Response.Store;
using ZivraFramework.Core.API.Exception;
using ZivraFramework.Core.Models;
using ZivraFramework.Core.Interfaces;
using ZivraFramework.Core.Utils;

namespace IdentityService.Infrastructure.Services;

public class StoreService : IStoreService
{
    private readonly IStoreRepository _storeRepo;
    private readonly IUserRepository _userRepo;
    private readonly IBranchRepository _branchRepo;
    private readonly ITokenService _tokenService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IUnitOfWork _unitOfWork;

    public StoreService(
        IStoreRepository storeRepo, 
        IUnitOfWork unitOfWork, 
        IUserRepository userRepo, 
        IBranchRepository branchRepo, 
        ITokenService tokenService, 
        IRefreshTokenService refreshTokenService)
    {
        _storeRepo = storeRepo;
        _unitOfWork = unitOfWork;
        _userRepo = userRepo;
        _branchRepo = branchRepo;
        _tokenService = tokenService;
        _refreshTokenService = refreshTokenService;
    }

    #region GetAllAsync
    public async Task<IEnumerable<StoreResponse>> GetAllAsync(PagedQuery query)
    {
        var paged = await _storeRepo.GetPagedAsync(query);
        var resList = paged.Items.Select(s => StoreMapper.ToResponse(s)).ToList();
        return resList;
    }
    #endregion

    #region GetByIdAsync
    public async Task<StoreResponse> GetByIdAsync(Guid id)
    {
        var store = await _storeRepo.GetByIdAsync(id);
        if (store == null) 
            throw new NotFoundException("Toko tidak ditemukan.");
        return StoreMapper.ToResponse(store);
    }
    #endregion

    #region GetByHashedIdAsync
    public async Task<StoreResponse> GetByHashedIdAsync(string hashedId)
    {
        var store = await _storeRepo.GetByHashedIdAsync(hashedId);
        if (store == null)
            throw new NotFoundException("Toko tidak ditemukan.");
        return StoreMapper.ToResponse(store);
    }
    #endregion

    #region CreateAsync
    public async Task<StoreResponse> CreateAsync(StoreRequest req)
    {
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var user = await _userRepo.GetUserWithRolesAsync(req.UserId);
            if (user == null)
                throw new NotFoundException("User tidak ditemukan.");
            
            if (user.RoleNames.All(ur => ur is not "OWNER"))
                throw new ForbiddenException("Hanya owner yang dapat membuat toko.");
            
            var lastCode = await _storeRepo.GetLastCodeAsync();
            lastCode = string.IsNullOrEmpty(lastCode) ? "0000000000000000" : (long.Parse(lastCode) + 1).ToString("D16");

            var store = new Store
            {
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
                CreDate = req.CreDate ?? DateTime.UtcNow,
                CreBy = req.CreBy,
                CreIpAddress = req.CreIpAddress
            };
            
            await _storeRepo.AddAsync(store);
            await _unitOfWork.SaveChangesAsync();
            
            var branch = new Branch
            {
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
            await _unitOfWork.SaveChangesAsync();
            
            await _userRepo.UpdateStoreIdAsync(user.Id ?? Guid.Empty, store.Id);
            await _unitOfWork.SaveChangesAsync();
            
            var newUser = await _userRepo.GetByIdAsync(user.Id);
            
            if (newUser == null)
                throw new NotFoundException("User tidak ditemukan setelah pembaruan.");

            await _tokenService.RevokeAllAccessTokensForUserAsync(user.Id ?? Guid.Empty, req.CreIpAddress);
            await _refreshTokenService.RevokeAllRefreshTokensForUserAsync(user.Id ?? Guid.Empty, req.CreIpAddress);
            
            newUser.HashedStoreId = store.HashedId;
            var token = await _tokenService.GenerateJwtToken(newUser, user.RoleNames);
            var refreshRaw = await _refreshTokenService.GenerateAndStoreRefreshTokenAsync(user.Id ?? Guid.Empty, token.Id);

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            Logger.Info("Toko berhasil dibuat dengan Id: " + store.Id);

            var response = StoreMapper.ToResponse(store);
            response.AccessToken = token.Token;
            response.RefreshToken = refreshRaw;

            return response;
        }
        catch (Exception e)
        {
            await _unitOfWork.RollbackTransactionAsync();
            Logger.Error("Gagal membuat toko", e);
            throw;
        }
    }
    #endregion

    #region UpdateAsync
    public async Task<StoreResponse> UpdateAsync(StoreRequest req)
    {
        var store = await _storeRepo.GetByIdAsync(req.Id);
        if (store == null)
            throw new NotFoundException("Toko tidak ditemukan.");

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

        Logger.Info("Toko berhasil diperbarui dengan Id: " + store.Id);
        return StoreMapper.ToResponse(store);
    }
    #endregion

    #region DeleteAsync
    public async Task<string> DeleteAsync(Guid id)
    {
        var store = await _storeRepo.GetByIdAsync(id);
        if (store == null)
            throw new NotFoundException("Toko tidak ditemukan.");

        _storeRepo.Delete(store);
        await _unitOfWork.SaveChangesAsync();

        Logger.Info("Toko berhasil dihapus dengan Id: " + id);
        return "Berhasil dihapus";
    }
    #endregion
}
