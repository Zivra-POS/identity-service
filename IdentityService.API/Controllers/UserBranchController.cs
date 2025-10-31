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

        var r = await _userBranchService.GetAllAsync(query, _currentUserService.StoreId.Value);
        
        if (!r.IsSuccess)
            return StatusCode((int)r.StatusCode, r);

        return Ok(r);
    }
    #endregion

    #region GetById
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var r = await _userBranchService.GetByIdAsync(id);
        
        if (!r.IsSuccess)
            return StatusCode((int)r.StatusCode, r);

        return Ok(r);
    }
    #endregion

    #region GetByUserId
    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetByUserId(Guid userId)
    {
        var r = await _userBranchService.GetByUserIdAsync(userId);
        
        if (!r.IsSuccess) return StatusCode((int)r.StatusCode, r);

        return Ok(r);
    }
    #endregion

    #region GetByBranchId
    [HttpGet("branch/{branchId}")]
    public async Task<IActionResult> GetByBranchId(Guid branchId)
    {
        var r = await _userBranchService.GetByBranchIdAsync(branchId);
        
        if (!r.IsSuccess)
            return StatusCode((int)r.StatusCode, r);

        return Ok(r);
    }
    #endregion

    #region Create
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] UserBranchRequest request)
    {
        var r = await _userBranchService.CreateAsync(request);
        
        if (!r.IsSuccess)
            return StatusCode((int)r.StatusCode, r);

        return CreatedAtAction(nameof(GetById), new { id = r.Data!.Id }, r);
    }
    #endregion

    #region Delete
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var r = await _userBranchService.DeleteAsync(id);
        
        if (!r.IsSuccess)
            return StatusCode((int)r.StatusCode, r);

        return Ok(r);
    }
    #endregion
}
