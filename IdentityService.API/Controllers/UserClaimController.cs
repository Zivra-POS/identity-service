using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using IdentityService.Core.Interfaces.Services;
using IdentityService.Shared.DTOs.Request.User;

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
        var res = await _service.GetByUserIdAsync(userId);
        return Ok(res);
    }
    #endregion

    #region GetById
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var res = await _service.GetByIdAsync(id);
        return Ok(res);
    }
    #endregion

    #region GetByHashedId
    [HttpGet("hashed/{hashedId}")]
    public async Task<IActionResult> GetByHashedId(string hashedId)
    {
        var res = await _service.GetByHashedIdAsync(hashedId);
        return Ok(res);
    }
    #endregion

    #region Create
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] UserClaimRequest req)
    {
        var res = await _service.CreateAsync(req);
        return StatusCode(StatusCodes.Status201Created, res);
    }
    #endregion

    #region Update
    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UserClaimRequest req)
    {
        var res = await _service.UpdateAsync(req);
        return Ok(res);
    }
    #endregion

    #region Delete
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var msg = await _service.DeleteAsync(id);
        return Ok(new { message = msg });
    }
    #endregion
}
