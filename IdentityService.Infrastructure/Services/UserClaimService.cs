using IdentityService.Core.Entities;
using IdentityService.Core.Interfaces.Repositories;
using IdentityService.Core.Interfaces.Services;
using IdentityService.Core.Mappers.User;
using IdentityService.Shared.DTOs.Request.User;
using IdentityService.Shared.DTOs.Response.User;
using ZivraFramework.Core.API.Exception;
using ZivraFramework.Core.Interfaces;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System;

namespace IdentityService.Infrastructure.Services;

public class UserClaimService : IUserClaimService
{
    private readonly IUserClaimRepository _ucRepo;
    private readonly IUserRepository _userRepo;
    private readonly IUnitOfWork _unitOfWork;

    public UserClaimService(IUserClaimRepository ucRepo, IUserRepository userRepo, IUnitOfWork unitOfWork)
    {
        _ucRepo = ucRepo;
        _userRepo = userRepo;
        _unitOfWork = unitOfWork;
    }

    #region GetByUserIdAsync
    public async Task<IEnumerable<UserClaimResponse>> GetByUserIdAsync(Guid userId)
    {
        var rows = await _ucRepo.GetByUserIdAsync(userId);
        var res = rows.Select(UserClaimMapper.ToResponse);
        return res;
    }
    #endregion

    #region GetByIdAsync
    public async Task<UserClaimResponse> GetByIdAsync(Guid id)
    {
        var uc = await _ucRepo.GetByIdAsync(id);
        if (uc == null)
            throw new NotFoundException("User Claim tidak ditemukan.");

        return UserClaimMapper.ToResponse(uc);
    }
    #endregion

    #region GetByHashedIdAsync
    public async Task<UserClaimResponse> GetByHashedIdAsync(string hashedId)
    {
        var uc = await _ucRepo.GetByHashedIdAsync(hashedId);
        if (uc == null)
            throw new NotFoundException("User Claim tidak ditemukan.");

        return UserClaimMapper.ToResponse(uc);
    }
    #endregion

    #region CreateAsync
    public async Task<UserClaimResponse> CreateAsync(UserClaimRequest req)
    {
        if (req.UserId == Guid.Empty)
            throw new ValidationException("User Id wajib diisi.");

        var user = await _userRepo.GetByIdAsync(req.UserId);
        if (user == null)
            throw new NotFoundException("User tidak ditemukan.");

        var uc = new UserClaim
        {
            Id = req.Id ?? Guid.Empty,
            UserId = req.UserId,
            ClaimType = req.ClaimType,
            ClaimValue = req.ClaimValue,
            CreDate = DateTime.UtcNow
        };

        await _ucRepo.AddAsync(uc);
        await _unitOfWork.SaveChangesAsync();

        return UserClaimMapper.ToResponse(uc);
    }
    #endregion

    #region UpdateAsync
    public async Task<UserClaimResponse> UpdateAsync(UserClaimRequest req)
    {
        if (req.Id == Guid.Empty)
            throw new ValidationException("Id wajib diisi.");

        var uc = await _ucRepo.GetByIdAsync(req.Id);
        if (uc == null)
            throw new NotFoundException("User Claim tidak ditemukan.");

        uc.ClaimType = req.ClaimType;
        uc.ClaimValue = req.ClaimValue;
        uc.ModDate = DateTime.UtcNow;

        _ucRepo.Update(uc);
        await _unitOfWork.SaveChangesAsync();

        return UserClaimMapper.ToResponse(uc);
    }
    #endregion

    #region DeleteAsync
    public async Task<string> DeleteAsync(Guid id)
    {
        var uc = await _ucRepo.GetByIdAsync(id);
        if (uc == null)
            throw new NotFoundException("User Claim tidak ditemukan.");

        _ucRepo.Delete(uc);
        await _unitOfWork.SaveChangesAsync();

        return "Deleted";
    }
    #endregion

    // Explicit interface implementations to avoid type identity ambiguities
    async Task<UserClaimResponse> IdentityService.Core.Interfaces.Services.IUserClaimService.CreateAsync(UserClaimRequest req)
    {
        return await CreateAsync(req);
    }

    async Task<UserClaimResponse> IdentityService.Core.Interfaces.Services.IUserClaimService.UpdateAsync(UserClaimRequest req)
    {
        return await UpdateAsync(req);
    }
}
