using Microsoft.AspNetCore.Mvc;
using IdentityService.Core.Interfaces.Services;
using IdentityService.Shared.DTOs.Request.Store;
using ZivraFramework.Core.Models;
using Microsoft.AspNetCore.Http;

namespace IdentityService.API.Controllers;

[ApiController]
[Route("api/stores")]
public class StoreController : ControllerBase
{
    private readonly IStoreService _storeService;
    private readonly ICurrentUserService _currentUser;
    private readonly IConfiguration _config;

    public StoreController(IStoreService storeService, ICurrentUserService currentUser, IConfiguration config)
    {
        _storeService = storeService;
        _currentUser = currentUser;
        _config = config;
    }

    #region GetAll
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PagedQuery query)
    {
        var res = await _storeService.GetAllAsync(query);
        return Ok(res);
    }
    #endregion

    #region GetById
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var res = await _storeService.GetByIdAsync(id);
        return Ok(res);
    }
    #endregion

    #region GetByHashedId
    [HttpGet("hashed/{hashedId}")]
    public async Task<IActionResult> GetByHashedId(string hashedId)
    {
        var res = await _storeService.GetByHashedIdAsync(hashedId);
        return Ok(res);
    }
    #endregion

    #region Create
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] StoreRequest model)
    {
        model.UserId = _currentUser.UserId;
        var res = await _storeService.CreateAsync(model);

        var jwtHours = int.TryParse(_config["JwtSettings:ExpireHours"], out var h) ? h : 3;
        var refreshDays = int.TryParse(_config["JwtSettings:RefreshTokenDays"], out var d) ? d : 30;

        var accessCookieOptions = new CookieOptions
        {
            HttpOnly = false,
            Secure = false,
            SameSite = SameSiteMode.Lax,
            Expires = DateTime.UtcNow.AddHours(jwtHours),
            Path = "/",
            Domain = null
        };

        var refreshCookieOptions = new CookieOptions
        {
            HttpOnly = false,
            Secure = false,
            SameSite = SameSiteMode.Lax,
            Expires = DateTime.UtcNow.AddDays(refreshDays),
            Path = "/",
            Domain = null
        };

        if (res.AccessToken != null) Response.Cookies.Append("access_token", res.AccessToken, accessCookieOptions);
        if (res.RefreshToken != null) Response.Cookies.Append("refresh_token", res.RefreshToken, refreshCookieOptions);

        return StatusCode(StatusCodes.Status201Created, res);
    }
    #endregion

    #region Update
    [HttpPut]
    public async Task<IActionResult> Update([FromBody] StoreRequest req)
    {
        var res = await _storeService.UpdateAsync(req);
        return Ok(res);
    }
    #endregion

    #region Delete
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var msg = await _storeService.DeleteAsync(id);
        return Ok(new { message = msg });
    }
    #endregion
}
