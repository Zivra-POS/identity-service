using IdentityService.Core.Entities;
using IdentityService.Core.Interfaces.Repositories;
using IdentityService.Core.Interfaces.Services;
using IdentityService.Core.Mappers.Role;
using IdentityService.Shared.DTOs.Request.Role;
using IdentityService.Shared.DTOs.RoleClaim;
using IdentityService.Shared.Response;
using ZivraFramework.Core.API.Exception;
using ZivraFramework.Core.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Threading.Tasks;

namespace IdentityService.Infrastructure.Services;

public class RoleClaimService : IRoleClaimService
{
    private readonly IRoleClaimRepository _rcRepo;
    private readonly IRoleRepository _roleRepo;
    private readonly IUnitOfWork _unitOfWork;

    public RoleClaimService(IRoleClaimRepository rcRepo, IRoleRepository roleRepo, IUnitOfWork unitOfWork)
    {
        _rcRepo = rcRepo;
        _roleRepo = roleRepo;
        _unitOfWork = unitOfWork;
    }

    #region GetByRoleIdAsync
    public async Task<IEnumerable<RoleClaimResponse>> GetByRoleIdAsync(Guid roleId)
    {
        var rows = await _rcRepo.GetByRoleIdAsync(roleId);
        var res = rows.Select(RoleClaimMapper.ToResponse);
        return res;
    }
    #endregion

    #region GetByIdAsync
    public async Task<RoleClaimResponse> GetByIdAsync(Guid id)
    {
        var rc = await _rcRepo.GetByIdAsync(id);
        if (rc == null)
            throw new NotFoundException("Role Claim tidak ditemukan.");

        return RoleClaimMapper.ToResponse(rc);
    }
    #endregion

    #region GetByHashedIdAsync
    public async Task<RoleClaimResponse> GetByHashedIdAsync(string hashedId)
    {
        var rc = await _rcRepo.GetByHashedIdAsync(hashedId);
        if (rc == null)
            throw new NotFoundException("Role Claim tidak ditemukan.");

        return RoleClaimMapper.ToResponse(rc);
    }
    #endregion

    #region CreateAsync
    public async Task<RoleClaimResponse> CreateAsync(RoleClaimRequest req)
    {
        if (req.RoleId == Guid.Empty)
            throw new ValidationException("Role Id is required.");

        var role = await _roleRepo.GetByIdAsync(req.RoleId);
        if (role == null)
            throw new NotFoundException("Role tidak ditemukan.");

        var rc = new RoleClaim
        {
            Id = Guid.NewGuid(),
            RoleId = req.RoleId,
            ClaimType = req.ClaimType,
            ClaimValue = req.ClaimValue,
            CreDate = DateTime.UtcNow
        };

        await _rcRepo.AddAsync(rc);
        await _unitOfWork.SaveChangesAsync();

        return RoleClaimMapper.ToResponse(rc);
    }
    #endregion

    #region UpdateAsync
    public async Task<RoleClaimResponse> UpdateAsync(RoleClaimRequest req)
    {
        if (req.Id == Guid.Empty)
            throw new ValidationException("Id is required.");

        var rc = await _rcRepo.GetByIdAsync(req.Id);
        if (rc == null)
            throw new NotFoundException("Role Claim tidak ditemukan.");

        rc.ClaimType = req.ClaimType;
        rc.ClaimValue = req.ClaimValue;
        rc.ModDate = DateTime.UtcNow;

        _rcRepo.Update(rc);
        await _unitOfWork.SaveChangesAsync();

        return RoleClaimMapper.ToResponse(rc);
    }
    #endregion

    #region DeleteAsync
    public async Task<string> DeleteAsync(Guid id)
    {
        var rc = await _rcRepo.GetByIdAsync(id);
        if (rc == null)
            throw new NotFoundException("Role Claim tidak ditemukan.");

        _rcRepo.Delete(rc);
        await _unitOfWork.SaveChangesAsync();

        return "Deleted";
    }
    #endregion
}
