using IdentityService.Core.Interfaces.Services;
using IdentityService.Infrastructure.Services;
using IdentityService.Shared.DTOs.Request.UserRole;
using Microsoft.AspNetCore.Mvc;
using ZivraFramework.Core.Filtering.Entities;
using ZivraFramework.Core.Utils;

namespace IdentityService.API.Controllers;

[ApiController]
[Route("api/user-roles")]
public class UserRoleController : ControllerBase
{
    private readonly IUserRoleService _service;
    private readonly ICurrentUserService _currentUserService;
    
    public UserRoleController(
        IUserRoleService service,
        ICurrentUserService currentUserService)
    {
        _service = service;
        _currentUserService = currentUserService;
    }
    
    #region GetAllAsync
    [HttpPost("search")]
    public async Task<IActionResult> GetAllAsync([FromBody] QueryRequest query)
    {
        var userIdFilter = query.Filters
            .FirstOrDefault(f => f.Field.Equals("UserId", StringComparison.OrdinalIgnoreCase));
        
        var userIdString = userIdFilter?.Value?.ToString();
        
        if (userIdString == null)
        {
            throw new ArgumentException("Staff Id tidak ditemukan.");
        }
        
        var userId = Base62Guid.Decode(userIdString, "u_");
        
        var res = await _service.GetAllAsync(query, userId);
        return Ok(res);
    }
    #endregion
    
    #region CreateBulkAsync
    [HttpPost]
    public async Task<IActionResult> CreateBulkAsync([FromBody] CreateUserRoleRequest req)
    {
        var r = await _service.CreateBulkAsync(req);
        return StatusCode(StatusCodes.Status201Created, new { count = r });
    }
    #endregion
    
    #region DeleteBulkAsync

    [HttpDelete]
    public async Task<IActionResult> DeleteBulkAsync([FromBody] List<Guid> ids)
    {
        if (_currentUserService?.UserId == null)
        {
            throw new UnauthorizedAccessException();
        }

        var r = await _service.DeleteBulkAsync(ids);
        return Ok(new { count = r });
    }
    #endregion
}