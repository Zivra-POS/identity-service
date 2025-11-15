using IdentityService.Core.Entities;
using IdentityService.Core.Interfaces.Repositories;
using IdentityService.Core.Interfaces.Services;
using IdentityService.Core.Mappers.Role;
using IdentityService.Shared.DTOs.Request.Role;
using IdentityService.Shared.DTOs.Response.Role;
using ZivraFramework.Core.API.Exception;
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
    public async Task<IEnumerable<RoleResponse>> GetAllAsync(PagedQuery query)
    {
        var paged = await _roleRepo.GetPagedAsync(query);

        var resList = paged.Items.Select(item => RoleMapper.ToResponse(item)).ToList();

        return resList;
    }
    #endregion

    #region GetById
    public async Task<RoleResponse> GetByIdAsync(Guid id)
    {
        var r = await _roleRepo.GetByIdAsync(id);
        if (r == null) 
            throw new NotFoundException("Role tidak ditemukan.");
        return RoleMapper.ToResponse(r);
    }
    #endregion

    #region GetByHashedId
    public async Task<RoleResponse> GetByHashedIdAsync(string hashedId)
    {
        var r = await _roleRepo.GetByHashedIdAsync(hashedId);
        if (r == null)
            throw new NotFoundException("Role tidak ditemukan.");
        return RoleMapper.ToResponse(r);
    }
    #endregion

    #region Create
    public async Task<RoleResponse> CreateAsync(RoleRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Name))
            throw new ValidationException("Nama role harus diisi.");

        var existing = await _roleRepo.GetByNameAsync(req.Name.ToUpperInvariant());
        if (existing != null)
            throw new ValidationException("Nama Role sudah ada.");

        var role = new Role
        {
            Id = Guid.NewGuid(),
            Name = req.Name,
            NormalizedName = req.Name.ToUpperInvariant(),
            Description = req.Description,
            CreDate = req.CreDate ?? DateTime.UtcNow,
            CreBy = req.CreBy,
            CreIpAddress = req.CreIpAddress
        };

        await _roleRepo.AddAsync(role);
        await _unitOfWork.SaveChangesAsync();

        return RoleMapper.ToResponse(role);
    }
    #endregion

    #region Update
    public async Task<RoleResponse> UpdateAsync(RoleRequest req)
    {
        if (req.Id == Guid.Empty)
            throw new ValidationException("Role Id wajib diisi.");

        var r = await _roleRepo.GetByIdAsync(req.Id);
        if (r == null) 
            throw new NotFoundException("Role tidak ditemukan.");

        if (!string.Equals(r.Name, req.Name, StringComparison.OrdinalIgnoreCase))
        {
            var exists = await _roleRepo.GetByNameAsync(req.Name.ToUpperInvariant());
            if (exists != null && exists.Id != r.Id)
                throw new ValidationException("Role name already exists.");
        }

        r.Name = req.Name;
        r.NormalizedName = req.Name.ToUpperInvariant();
        r.Description = req.Description;
        r.ModDate = DateTime.UtcNow;

        _roleRepo.Update(r);
        await _unitOfWork.SaveChangesAsync();

        return RoleMapper.ToResponse(r);
    }
    #endregion

    #region Delete
    public async Task<string> DeleteAsync(Guid id)
    {
        var r = await _roleRepo.GetByIdAsync(id);
        if (r == null) 
            throw new NotFoundException("Role tidak ditemukan.");

        _roleRepo.Delete(r);
        await _unitOfWork.SaveChangesAsync();

        return "Deleted";
    }
    #endregion
}
