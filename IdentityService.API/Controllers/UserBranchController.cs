using IdentityService.Core.Interfaces.Services;
using IdentityService.Shared.DTOs.Request.UserBranch;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZivraFramework.Core.Models;

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
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PagedQuery query)
    {
        if (_currentUserService.StoreId == null)
            return Unauthorized("Store information is required");

        var res = await _userBranchService.GetAllAsync(query, Guid.Empty);
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
    public async Task<IActionResult> Create([FromBody] UserBranchRequest request)
    {
        var res = await _userBranchService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = res.Id }, res);
    }
    #endregion

    #region Delete
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var msg = await _userBranchService.DeleteAsync(id);
        return Ok(new { message = msg });
    }
    #endregion
}
