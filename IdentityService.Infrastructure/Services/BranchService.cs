using IdentityService.Core.Entities;
using IdentityService.Core.Interfaces.Repositories;
using IdentityService.Core.Interfaces.Services;
using IdentityService.Core.Mappers.Branch;
using IdentityService.Shared.DTOs.Request.Branch;
using IdentityService.Shared.DTOs.Response.Branch;
using IdentityService.Shared.Response;
using ZivraFramework.Core.Models;
using ZivraFramework.Core.Interfaces;

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
    public async Task<Result<IEnumerable<BranchResponse>>> GetAllAsync(PagedQuery query, Guid storeId)
    {
        var paged = await _branchRepo.GetPagedByStoreAsync(query, storeId);
        var resList = paged.Items.Select(b => BranchMapper.ToResponse(b)).ToList();
        return Result<IEnumerable<BranchResponse>>.Success(resList);
    }
    #endregion

    #region GetById
    public async Task<Result<BranchResponse>> GetByIdAsync(Guid id)
    {
        var b = await _branchRepo.GetByIdAsync(id);
        if (b == null) return Result<BranchResponse>.Failure(new List<string>{"Branch tidak ditemukan."}, "Not found");
        return Result<BranchResponse>.Success(BranchMapper.ToResponse(b));
    }
    #endregion

    #region Create
    public async Task<Result<BranchResponse>> CreateAsync(BranchRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Name))
            return Result<BranchResponse>.Failure(new List<string>{"Name is required."}, "Validation failed");

        var branch = new Branch
        {
            Id = req.Id ?? Guid.NewGuid(),
            StoreId = req.StoreId,
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
            CreDate = req.CreDate,
            CreBy = req.CreBy,
            CreIpAddress = req.CreIpAddress
        };

        await _branchRepo.AddAsync(branch);
        await _unitOfWork.SaveChangesAsync();

        return Result<BranchResponse>.Success(BranchMapper.ToResponse(branch));
    }
    #endregion

    #region Update
    public async Task<Result<BranchResponse>> UpdateAsync(BranchRequest req)
    {
        if (req.Id == null || req.Id == Guid.Empty)
            return Result<BranchResponse>.Failure(new List<string>{"Id is required."}, "Validation failed");

        var b = await _branchRepo.GetByIdAsync(req.Id.Value);
        if (b == null) return Result<BranchResponse>.Failure(new List<string>{"Branch tidak ditemukan."}, "Not found");

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

        return Result<BranchResponse>.Success(BranchMapper.ToResponse(b));
    }
    #endregion

    #region Delete
    public async Task<Result<string>> DeleteAsync(Guid id)
    {
        var b = await _branchRepo.GetByIdAsync(id);
        if (b == null) return Result<string>.Failure(new List<string>{"Branch tidak ditemukan."}, "Not found");

        _branchRepo.Delete(b);
        await _unitOfWork.SaveChangesAsync();

        return Result<string>.Success("Deleted");
    }
    #endregion
}

