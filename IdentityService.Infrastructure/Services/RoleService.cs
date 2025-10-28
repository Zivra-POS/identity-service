using IdentityService.Core.Entities;
using IdentityService.Core.Interfaces.Repositories;
using IdentityService.Core.Interfaces.Services;
using IdentityService.Core.Mappers.Role;
using IdentityService.Shared.DTOs.Request.Role;
using IdentityService.Shared.DTOs.Response.Role;
using IdentityService.Shared.Response;
using ZivraFramework.Core.Interfaces;
using ZivraFramework.Core.Models;

namespace IdentityService.Infrastructure.Services;

public class RoleService : IRoleService
{
    private readonly IRoleRepository _roleRepo;
    private readonly IUnitOfWork _unitOfWork;

    public RoleService(IRoleRepository roleRepo, IUnitOfWork unitOfWork)
    {
        _roleRepo = roleRepo;
        _unitOfWork = unitOfWork;
    }

    #region GetAll
    public async Task<Result<IEnumerable<RoleResponse>>> GetAllAsync(PagedQuery query)
    {
        var paged = await _roleRepo.GetPagedAsync(query);

        var resList = paged.Items.Select(item => RoleMapper.ToResponse(item)).ToList();

        return Result<IEnumerable<RoleResponse>>.Success(resList);
    }
    #endregion

    #region GetById
    public async Task<Result<RoleResponse>> GetByIdAsync(Guid id)
    {
        var r = await _roleRepo.GetByIdAsync(id);
        if (r == null) return Result<RoleResponse>.Failure(new List<string>{"Role tidak ditemukan."}, "Not found");
        return Result<RoleResponse>.Success(RoleMapper.ToResponse(r));
    }
    #endregion

    #region Create
    public async Task<Result<RoleResponse>> CreateAsync(RoleRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Name))
            return Result<RoleResponse>.Failure(new List<string>{"Name is required."}, "Validation failed");

        var existing = await _roleRepo.GetByNameAsync(req.Name.ToUpperInvariant());
        if (existing != null)
            return Result<RoleResponse>.Failure(new List<string>{"Role name already exists."}, "Validation failed");

        var role = new Role
        {
            Id = req.Id ?? Guid.NewGuid(),
            Name = req.Name,
            NormalizedName = req.Name.ToUpperInvariant(),
            Description = req.Description,
            CreDate = req.CreDate,
            CreBy = req.CreBy,
            CreIpAddress = req.CreIpAddress
        };

        await _roleRepo.AddAsync(role);
        await _unitOfWork.SaveChangesAsync();

        return Result<RoleResponse>.Success(RoleMapper.ToResponse(role));
    }
    #endregion

    #region Update
    public async Task<Result<RoleResponse>> UpdateAsync(RoleRequest req)
    {
        if (req.Id == null || req.Id == Guid.Empty)
            return Result<RoleResponse>.Failure(new List<string>{"Id is required."}, "Validation failed");

        var r = await _roleRepo.GetByIdAsync(req.Id.Value);
        if (r == null) return Result<RoleResponse>.Failure(new List<string>{"Role tidak ditemukan."}, "Not found");

        if (!string.Equals(r.Name, req.Name, StringComparison.OrdinalIgnoreCase))
        {
            var exists = await _roleRepo.GetByNameAsync(req.Name.ToUpperInvariant());
            if (exists != null && exists.Id != r.Id)
                return Result<RoleResponse>.Failure(new List<string>{"Role name already exists."}, "Validation failed");
        }

        r.Name = req.Name;
        r.NormalizedName = req.Name.ToUpperInvariant();
        r.Description = req.Description;
        r.ModDate = DateTime.UtcNow;

        _roleRepo.Update(r);
        await _unitOfWork.SaveChangesAsync();

        return Result<RoleResponse>.Success(RoleMapper.ToResponse(r));
    }
    #endregion

    #region Delete
    public async Task<Result<string>> DeleteAsync(Guid id)
    {
        var r = await _roleRepo.GetByIdAsync(id);
        if (r == null) return Result<string>.Failure(new List<string>{"Role tidak ditemukan."}, "Not found");

        _roleRepo.Delete(r);
        await _unitOfWork.SaveChangesAsync();

        return Result<string>.Success("Deleted");
    }
    #endregion
}
