using Microsoft.AspNetCore.Mvc;
using IdentityService.Core.Interfaces.Services;
using IdentityService.Shared.DTOs.Request.Store;
using ZivraFramework.Core.Models;

namespace IdentityService.API.Controllers;

[ApiController]
[Route("api/stores")]
public class StoreController : ControllerBase
{
    private readonly IStoreService _storeService;
    private readonly ICurrentUserService _currentUser;

    public StoreController(IStoreService storeService, ICurrentUserService currentUser)
    {
        _storeService = storeService;
        _currentUser = currentUser;
    }

    #region GetAll
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PagedQuery query)
    {
        var r = await _storeService.GetAllAsync(query);
        return StatusCode((int)r.StatusCode, r);
    }
    #endregion

    #region GetById
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var r = await _storeService.GetByIdAsync(id);
        return StatusCode((int)r.StatusCode, r);
    }
    #endregion

    #region Create
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] StoreRequest model)
    {
        model.UserId = _currentUser.UserId;
        var r = await _storeService.CreateAsync(model);
        return StatusCode((int)r.StatusCode, r);
    }
    #endregion

    #region Update
    [HttpPut]
    public async Task<IActionResult> Update([FromBody] StoreRequest req)
    {
        var r = await _storeService.UpdateAsync(req);
        return StatusCode((int)r.StatusCode, r);
    }
    #endregion

    #region Delete
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var r = await _storeService.DeleteAsync(id);
        return StatusCode((int)r.StatusCode, r);
    }
    #endregion
}
