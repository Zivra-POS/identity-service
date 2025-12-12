using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using IdentityService.Core.Interfaces.Services;
using IdentityService.Shared.DTOs.Request.Branch;
using ZivraFramework.Core.API.Exception;
using ZivraFramework.Core.Filtering.Entities;
using ZivraFramework.Core.Models;
using ZivraFramework.Core.Utils;

namespace IdentityService.API.Controllers;

[ApiController]
[Route("api/branches")]
public class BranchController : ControllerBase
{
    private readonly IBranchService _branchService;
    private readonly ICurrentUserService _currentUserService;

    public BranchController(IBranchService branchService, ICurrentUserService currentUserService)
    {
        _branchService = branchService;
        _currentUserService = currentUserService;
    }

    #region GetAll
    [HttpPost("search")]
    public async Task<IActionResult> GetAll([FromBody] QueryRequest query)
    {
        var storeId = _currentUserService.StoreId;
        
        if (storeId == null)
        {
            throw new ForbiddenException("Anda tidak memiliki Toko untuk mengakses data cabang.");
        }
        
        var res = await _branchService.GetALlByStoreId(query, storeId);
        return Ok(res);
    }
    #endregion

    #region GetById
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var res = await _branchService.GetByIdAsync(id);
        return Ok(res);
    }
    #endregion

    #region GetByHashedId
    [HttpGet("hashed/{hashedId}")]
    public async Task<IActionResult> GetByHashedId(string hashedId)
    {
        var res = await _branchService.GetByHashedIdAsync(hashedId);
        return Ok(res);
    }
    #endregion

    #region Create
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] BranchRequest req)
    {
        req.StoreId = _currentUserService.StoreId;
        var res = await _branchService.CreateAsync(req);
        return StatusCode(StatusCodes.Status201Created, res);
    }
    #endregion

    #region Update
    [HttpPut]
    public async Task<IActionResult> Update([FromBody] BranchRequest req)
    {
        if (req.HashedId == null)
        {
            throw new ValidationException("Id tidak ditemukan.");
        }
        
        req.Id = Base62Guid.Decode(req.HashedId, "b_");
        var res = await _branchService.UpdateAsync(req);
        return Ok(res);
    }
    #endregion

    #region Delete
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var msg = await _branchService.DeleteAsync(id);
        return Ok(new { message = msg });
    }
    #endregion
    
    #region DeleteByListId
    [HttpDelete]
    public async Task<IActionResult> DeleteByListId([FromBody] List<Guid> ids)
    {
        var deletedCount = await _branchService.DeleteByListIdAsync(ids);
        return Ok(new { deletedCount });
    }
    #endregion
}
