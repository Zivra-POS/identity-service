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

        var result = await _userBranchService.GetAllAsync(query, _currentUserService.StoreId.Value);
        
        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }
    #endregion

    #region GetById
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _userBranchService.GetByIdAsync(id);
        
        if (!result.IsSuccess)
            return NotFound(result);

        return Ok(result);
    }
    #endregion

    #region GetByUserId
    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetByUserId(Guid userId)
    {
        var result = await _userBranchService.GetByUserIdAsync(userId);
        
        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }
    #endregion

    #region GetByBranchId
    [HttpGet("branch/{branchId}")]
    public async Task<IActionResult> GetByBranchId(Guid branchId)
    {
        var result = await _userBranchService.GetByBranchIdAsync(branchId);
        
        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }
    #endregion

    #region Create
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] UserBranchRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _userBranchService.CreateAsync(request);
        
        if (!result.IsSuccess)
            return BadRequest(result);

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }
    #endregion

    #region Delete
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _userBranchService.DeleteAsync(id);
        
        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }
    #endregion

    #region GetRowsForLookup
    [HttpGet("lookup")]
    public async Task<IActionResult> GetRowsForLookup()
    {
        if (_currentUserService.StoreId == null)
            return Unauthorized("Store information is required");

        var result = await _userBranchService.GetRowsForLookupAsync(_currentUserService.StoreId.Value);
        
        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }
    #endregion
}
