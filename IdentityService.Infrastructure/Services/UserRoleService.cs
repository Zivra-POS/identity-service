using IdentityService.Core.Interfaces.Repositories;
using IdentityService.Core.Interfaces.Services;
using IdentityService.Shared.DTOs.Request.UserRole;
using IdentityService.Shared.DTOs.Response.UserRole;
using ZivraFramework.Core.Filtering.Entities;
using ZivraFramework.Core.Interfaces;
using ZivraFramework.Core.Models;
using ZivraFramework.Core.Utils;

namespace IdentityService.Infrastructure.Services;

public class UserRoleService : IUserRoleService
{
    private readonly IUserRoleRepository _userRoleRepository;
    private readonly IUnitOfWork _unitOfWork;
    
    public UserRoleService(
        IUserRoleRepository userRoleRepository, 
        IUnitOfWork unitOfWork)
    {
        _userRoleRepository = userRoleRepository;
        _unitOfWork = unitOfWork;
    }
    
    #region GetAllAsync
    public async Task<PagedResult<UserRoleResponse>> GetAllAsync(QueryRequest query, Guid userId)
    {
        return await _userRoleRepository.GetAllAsync(query, userId);
    }
    #endregion
    
    #region CreateBulkAsync
    public async Task<int> CreateBulkAsync(CreateUserRoleRequest req)
    {
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var userId = Base62Guid.Decode(req.UserId ?? string.Empty, "u_");
            var existingUserRoles = await _userRoleRepository.GetRowsByUserIdAsync(userId);
            var existingRoleIds = existingUserRoles.Select(ur => ur.RoleId).ToHashSet();

            foreach (var roleId in req?.RoleIds!)
            {
                if (!existingRoleIds.Contains(roleId))
                {
                    var userRole = new Core.Entities.UserRole
                    {
                        UserId = userId,
                        RoleId = roleId
                    };
                    await _userRoleRepository.AddAsync(userRole);
                }
            }

            var result = await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();
            return result;
        }
        catch (Exception e)
        {
            Logger.Error("Gagal menambahkan role ke user", e);
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }
    #endregion
    
    #region DeleteBulkAsync
    public async Task<int> DeleteBulkAsync(List<Guid> ids)
    {
        await _unitOfWork.BeginTransactionAsync();
        try
        { 
            foreach (var id in ids)
            {
                var userRole = await _userRoleRepository.GetByIdAsync(id);
                if (userRole != null)
                {
                    _userRoleRepository.Delete(userRole);
                }
            }

            var result = await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();
            return result;
        }
        catch (Exception e)
        {
            Logger.Error("Gagal menghapus role dari user", e);
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }
    #endregion
}