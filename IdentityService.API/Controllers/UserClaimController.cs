using Microsoft.AspNetCore.Mvc;
using IdentityService.Core.Interfaces.Services;
using IdentityService.Shared.DTOs.UserClaim;

namespace IdentityService.API.Controllers;

[ApiController]
[Route("api/user-claims")]
public class UserClaimController : ControllerBase
{
    private readonly IUserClaimService _service;

    public UserClaimController(IUserClaimService service)
    {
        _service = service;
    }

    #region GetByUser
    [HttpGet("user/{userId:guid}")]
    public async Task<IActionResult> GetByUser(Guid userId)
    {
        var r = await _service.GetByUserIdAsync(userId);
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
    public async Task<IActionResult> Create([FromBody] UserClaimRequest req)
    {
        var r = await _service.CreateAsync(req);
        return StatusCode((int)r.StatusCode, r);
    }
    #endregion

    #region Update
    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UserClaimRequest req)
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

