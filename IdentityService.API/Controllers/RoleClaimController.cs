using Microsoft.AspNetCore.Mvc;
using IdentityService.Core.Interfaces.Services;
using IdentityService.Shared.DTOs.Request.Role;

namespace IdentityService.API.Controllers;

[ApiController]
[Route("api/role-claims")]
public class RoleClaimController : ControllerBase
{
    private readonly IRoleClaimService _service;

    public RoleClaimController(IRoleClaimService service)
    {
        _service = service;
    }

    #region GetByRole
    [HttpGet("role/{roleId:guid}")]
    public async Task<IActionResult> GetByRole(Guid roleId)
    {
        var r = await _service.GetByRoleIdAsync(roleId);
        return StatusCode((int)r.StatusCode, r);
    }
    #endregion

    #region GetById
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var r = await _service.GetByIdAsync(id);
        return StatusCode((int)r.StatusCode, r);
    }
    #endregion

    #region Create
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] RoleClaimRequest req)
    {
        var r = await _service.CreateAsync(req);
        return StatusCode((int)r.StatusCode, r);
    }
    #endregion

    #region Update
    [HttpPut]
    public async Task<IActionResult> Update([FromBody] RoleClaimRequest req)
    {
        var r = await _service.UpdateAsync(req);
        return StatusCode((int)r.StatusCode, r);
    }
    #endregion

    #region Delete
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var r = await _service.DeleteAsync(id);
        return StatusCode((int)r.StatusCode, r);
    }
    #endregion
}
