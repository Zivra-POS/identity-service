using IdentityService.Core.Entities;
using IdentityService.Core.Interfaces.Repositories;
using IdentityService.Core.Interfaces.Services;
using IdentityService.Core.Mappers.Branch;
using IdentityService.Shared.DTOs.Request.Branch;
using IdentityService.Shared.DTOs.Response.Branch;
using ZivraFramework.Core.API.Exception;
using ZivraFramework.Core.Models;
using ZivraFramework.Core.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System;
using ZivraFramework.Core.Filtering.Entities;
using ZivraFramework.Core.Utils;

namespace IdentityService.Infrastructure.Services;

public class BranchService : IBranchService
{
    private readonly IBranchRepository _branchRepo;
    private readonly IUnitOfWork _unitOfWork;

    public BranchService(IBranchRepository branchRepo, IUnitOfWork unitOfWork)
    {
        _branchRepo = branchRepo;
        _unitOfWork = unitOfWork;
    }

    #region GetAll
    public async Task<IEnumerable<BranchResponse>> GetAllAsync(PagedQuery query, Guid storeId)
    {
        var paged = await _branchRepo.GetPagedByStoreAsync(query, storeId);
        var resList = paged.Items.Select(b => BranchMapper.ToResponse(b)).ToList();
        return resList;
    }
    #endregion
    
    #region GetALlByStoreId
    public async Task<PagedResult<Branch>> GetALlByStoreId(QueryRequest query, string storeId)
    {
        var storeIdDecode = Base62Guid.Decode(storeId, "s_");
        var paged = await _branchRepo.GetAllByStoreIdAsync(query, storeIdDecode);
        return paged;
    }
    #endregion

    #region GetById
    public async Task<BranchResponse> GetByIdAsync(Guid id)
    {
        var b = await _branchRepo.GetByIdAsync(id);
        if (b == null) 
            throw new NotFoundException("Branch tidak ditemukan.");
        return BranchMapper.ToResponse(b);
    }
    #endregion

    #region Create
    public async Task<BranchResponse> CreateAsync(BranchRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Name))
            throw new ValidationException("Nama Cabang wajib diisi.");
        
        if (string.IsNullOrEmpty(req.StoreId)) 
            throw new ValidationException("Toko tidak ditemukan.");
        
        var storeIdDecode = Base62Guid.Decode(req.StoreId, "s_");

        var branch = new Branch
        {
            Id = Guid.NewGuid(),
            StoreId = storeIdDecode,
            Name = req.Name,
            Code = req.Code,
            Address = req.Address,
            Province = req.Province,
            City = req.City,
            District = req.District,
            Rt = req.Rt,
            Rw = req.Rw,
            Phone = req.Phone,
            IsActive = true,
            CreDate = req.CreDate ?? DateTime.UtcNow,
            CreBy = req.CreBy,
            CreIpAddress = req.CreIpAddress
        };

        await _branchRepo.AddAsync(branch);
        await _unitOfWork.SaveChangesAsync();

        return BranchMapper.ToResponse(branch);
    }
    #endregion

    #region Update
    public async Task<BranchResponse> UpdateAsync(BranchRequest req)
    {
        if (req.Id == Guid.Empty)
            throw new ValidationException("Id tidak ditemukan.");

        var b = await _branchRepo.GetByIdAsync(req.Id);
        if (b == null) 
            throw new NotFoundException("Branch tidak ditemukan.");

        b.Name = req.Name;
        b.Code = req.Code;
        b.Address = req.Address;
        b.Province = req.Province;
        b.City = req.City;
        b.District = req.District;
        b.Rt = req.Rt;
        b.Rw = req.Rw;
        b.Phone = req.Phone;
        b.ModDate = DateTime.UtcNow;

        _branchRepo.Update(b);
        await _unitOfWork.SaveChangesAsync();

        return BranchMapper.ToResponse(b);
    }
    #endregion

    #region Delete
    public async Task<string> DeleteAsync(Guid id)
    {
        var b = await _branchRepo.GetByIdAsync(id);
        if (b == null) 
            throw new NotFoundException("Branch tidak ditemukan.");

        _branchRepo.Delete(b);
        await _unitOfWork.SaveChangesAsync();

        return "Deleted";
    }
    #endregion

    #region DeleteByListId

    public async Task<int> DeleteByListIdAsync(List<Guid> ids)
    {
        if (ids == null || !ids.Any())
            throw new ValidationException("Daftar Id tidak ditemukan.");

        foreach (var id in ids)
        {
            var b = new Branch()
            {
                Id = id
            };

            _branchRepo.Delete(b);
        }

        return await _unitOfWork.SaveChangesAsync();
    }
    #endregion

    #region GetByHashedId
    public async Task<BranchResponse> GetByHashedIdAsync(string hashedId)
    {
        var b = await _branchRepo.GetByHashedIdAsync(hashedId);
        if (b == null)
            throw new NotFoundException("Branch tidak ditemukan.");
        return BranchMapper.ToResponse(b);
    }
    #endregion
}