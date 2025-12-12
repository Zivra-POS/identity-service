using IdentityService.Core.Interfaces.Services;
using IdentityService.Shared.DTOs.Request.UserBranch;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZivraFramework.Core.Filtering.Entities;
using ZivraFramework.Core.Models;
using ZivraFramework.Core.Utils;

namespace IdentityService.API.Controllers;

[ApiController]
[Route("api/user-branches")]
[Authorize]
public class UserBranchController : ControllerBase
{
    private readonly IUserBranchService _userBranchService;
    private readonly ICurrentUserService _currentUserService;

    public UserBranchController(IUserBranchService userBranchService, ICurrentUserService currentUserService)
    {
        _userBranchService = userBranchService;
        _currentUserService = currentUserService;
    }

    #region GetAll
    [HttpPost("search")]
    public async Task<IActionResult> GetAll([FromBody] QueryRequest query)
    {
        if (_currentUserService.StoreId == null)
            return Unauthorized("Toko Id tidak ditemukan.");

        var storeId = Base62Guid.Decode(_currentUserService.StoreId, "s_");

        var res = await _userBranchService.GetAllAsync(query, storeId);
        return Ok(res);
    }
    #endregion

    #region GetById
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var res = await _userBranchService.GetByIdAsync(id);
        return Ok(res);
    }
    #endregion

    #region GetByHashedId
    [HttpGet("hashed/{hashedId}")]
    public async Task<IActionResult> GetByHashedId(string hashedId)
    {
        var res = await _userBranchService.GetByHashedIdAsync(hashedId);
        return Ok(res);
    }
    #endregion

    #region GetByUserId
    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetByUserId(Guid userId)
    {
        var res = await _userBranchService.GetByUserIdAsync(userId);
        return Ok(res);
    }
    #endregion

    #region GetByBranchId
    [HttpGet("branch/{branchId}")]
    public async Task<IActionResult> GetByBranchId(Guid branchId)
    {
        var res = await _userBranchService.GetByBranchIdAsync(branchId);
        return Ok(res);
    }
    #endregion

    #region Create
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] IEnumerable<UserBranchRequest> request)
    {
        var r = await _userBranchService.CreateBulkAsync(request);
        return Ok(new { count = r });
    }
    #endregion

    #region Delete
    [HttpDelete]
    public async Task<IActionResult> Delete([FromBody] List<Guid> id)
    {
        var r = await _userBranchService.DeleteBulkAsync(id);
        return Ok(new { count = r });
    }
    #endregion
    
    #region ChangePrimaryBranch
    [HttpPost("change-primary")]
    public async Task<IActionResult> ChangePrimaryBranch([FromBody] ChangePrimaryBranchRequest request)
    {
        var r = await _userBranchService.ChangePrimaryBranchAsync(request);
        return Ok(new { count = r });
    }
    #endregion
}
