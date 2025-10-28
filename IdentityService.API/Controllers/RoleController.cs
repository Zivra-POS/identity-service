using Microsoft.AspNetCore.Mvc;
using IdentityService.Core.Interfaces.Services;
using IdentityService.Shared.DTOs.Request.Role;
using ZivraFramework.Core.Models;

namespace IdentityService.API.Controllers;

[ApiController]
[Route("api/roles")]
public class RoleController : ControllerBase
{
    private readonly IRoleService _roleService;

    public RoleController(IRoleService roleService)
    {
        _roleService = roleService;
    }

    #region GetAll
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PagedQuery query)
    {
        var r = await _roleService.GetAllAsync(query);
        return StatusCode((int)r.StatusCode, r);
    }
    #endregion

    #region GetById
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var r = await _roleService.GetByIdAsync(id);
        return StatusCode((int)r.StatusCode, r);
    }
    #endregion

    #region Create
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] RoleRequest req)
    {
        var r = await _roleService.CreateAsync(req);
        return StatusCode((int)r.StatusCode, r);
    }
    #endregion

    #region Update
    [HttpPut]
    public async Task<IActionResult> Update([FromBody] RoleRequest req)
    {
        var r = await _roleService.UpdateAsync(req);
        return StatusCode((int)r.StatusCode, r);
    }
    #endregion

    #region Delete
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var r = await _roleService.DeleteAsync(id);
        return StatusCode((int)r.StatusCode, r);
    }
    #endregion
}
