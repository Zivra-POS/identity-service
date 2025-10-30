using IdentityService.Core.Entities;
using IdentityService.Core.Interfaces.Repositories;
using IdentityService.Core.Interfaces.Services;
using IdentityService.Core.Mappers.Role;
using IdentityService.Shared.DTOs.Request.Role;
using IdentityService.Shared.DTOs.RoleClaim;
using IdentityService.Shared.Response;
using ZivraFramework.Core.Interfaces;

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
    public async Task<Result<IEnumerable<RoleClaimResponse>>> GetByRoleIdAsync(Guid roleId)
    {
        var rows = await _rcRepo.GetByRoleIdAsync(roleId);
        var res = rows.Select(RoleClaimMapper.ToResponse);
        return Result<IEnumerable<RoleClaimResponse>>.Success(res);
    }
    #endregion

    #region GetByIdAsync
    public async Task<Result<RoleClaimResponse>> GetByIdAsync(Guid id)
    {
        var rc = await _rcRepo.GetByIdAsync(id);
        if (rc == null)
            return Result<RoleClaimResponse>.Failure(new List<string> { "RoleClaim tidak ditemukan." }, "Not found");

        return Result<RoleClaimResponse>.Success(RoleClaimMapper.ToResponse(rc));
    }
    #endregion

    #region CreateAsync
    public async Task<Result<RoleClaimResponse>> CreateAsync(RoleClaimRequest req)
    {
        if (req.RoleId == Guid.Empty)
            return Result<RoleClaimResponse>.Failure(new List<string> { "RoleId is required." }, "Validation failed");

        var role = await _roleRepo.GetByIdAsync(req.RoleId);
        if (role == null)
            return Result<RoleClaimResponse>.Failure(new List<string> { "Role tidak ditemukan." }, "Validation failed");

        var rc = new RoleClaim
        {
            Id = req.Id ?? Guid.NewGuid(),
            RoleId = req.RoleId,
            ClaimType = req.ClaimType,
            ClaimValue = req.ClaimValue,
            CreDate = DateTime.UtcNow
        };

        await _rcRepo.AddAsync(rc);
        await _unitOfWork.SaveChangesAsync();

        return Result<RoleClaimResponse>.Success(RoleClaimMapper.ToResponse(rc));
    }
    #endregion

    #region UpdateAsync
    public async Task<Result<RoleClaimResponse>> UpdateAsync(RoleClaimRequest req)
    {
        if (req.Id == null || req.Id == Guid.Empty)
            return Result<RoleClaimResponse>.Failure(new List<string> { "Id is required." }, "Validation failed");

        var rc = await _rcRepo.GetByIdAsync(req.Id.Value);
        if (rc == null)
            return Result<RoleClaimResponse>.Failure(new List<string> { "RoleClaim tidak ditemukan." }, "Not found");

        rc.ClaimType = req.ClaimType;
        rc.ClaimValue = req.ClaimValue;
        rc.ModDate = DateTime.UtcNow;

        _rcRepo.Update(rc);
        await _unitOfWork.SaveChangesAsync();

        return Result<RoleClaimResponse>.Success(RoleClaimMapper.ToResponse(rc));
    }
    #endregion

    #region DeleteAsync
    public async Task<Result<string>> DeleteAsync(Guid id)
    {
        var rc = await _rcRepo.GetByIdAsync(id);
        if (rc == null)
            return Result<string>.Failure(new List<string> { "RoleClaim tidak ditemukan." }, "Not found");

        _rcRepo.Delete(rc);
        await _unitOfWork.SaveChangesAsync();

        return Result<string>.Success("Deleted");
    }
    #endregion
}
