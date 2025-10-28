using Microsoft.AspNetCore.Mvc;
using IdentityService.Core.Interfaces.Services;
using IdentityService.Shared.DTOs.Request.Branch;
using ZivraFramework.Core.Models;

namespace IdentityService.API.Controllers;

[ApiController]
[Route("api/branches")]
public class BranchController : ControllerBase
{
    private readonly IBranchService _branchService;

    public BranchController(IBranchService branchService)
    {
        _branchService = branchService;
    }

    #region GetAll
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PagedQuery query, [FromQuery] Guid storeId)
    {
        var r = await _branchService.GetAllAsync(query, storeId);
        return StatusCode((int)r.StatusCode, r);
    }
    #endregion

    #region GetById
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var r = await _branchService.GetByIdAsync(id);
        return StatusCode((int)r.StatusCode, r);
    }
    #endregion

    #region Create
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] BranchRequest req)
    {
        var r = await _branchService.CreateAsync(req);
        return StatusCode((int)r.StatusCode, r);
    }
    #endregion

    #region Update
    [HttpPut]
    public async Task<IActionResult> Update([FromBody] BranchRequest req)
    {
        var r = await _branchService.UpdateAsync(req);
        return StatusCode((int)r.StatusCode, r);
    }
    #endregion

    #region Delete
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var r = await _branchService.DeleteAsync(id);
        return StatusCode((int)r.StatusCode, r);
    }
    #endregion
}

