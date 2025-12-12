using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.Shared.DTOs.Request.Auth;

public class UpdateUserRequest
{
    [FromForm(Name = "HashedId")]
    public string? HashedId { get; set; }

    [FromForm(Name = "Id")]
    public Guid? Id { get; set; }

    [FromForm(Name = "CreDate")]
    public DateTime? CreDate { get; set; }

    [FromForm(Name = "CreBy")]
    public string? CreBy { get; set; }

    [FromForm(Name = "CreIpAddress")]
    public string? CreIpAddress { get; set; }

    [FromForm(Name = "ModDate")]
    public DateTime? ModDate { get; set; }

    [FromForm(Name = "ModBy")]
    public string? ModBy { get; set; }

    [FromForm(Name = "ModIpAddress")]
    public string? ModIpAddress { get; set; }

    [FromForm(Name = "FullName")]
    public string? FullName { get; set; }

    [FromForm(Name = "DisplayName")]
    public string? DisplayName { get; set; }

    [FromForm(Name = "PhoneNumber")]
    public string? PhoneNumber { get; set; }

    [FromForm(Name = "IsActive")]
    public bool? IsActive { get; set; }

    [FromForm(Name = "ProfileImage")]
    public IFormFile? ProfileImage { get; set; }
    
    [FromForm(Name = "RemoveImage")]
    public bool RemoveImage { get; set; }

    [MaxLength(4000)]
    [FromForm(Name = "Address")]
    public string? Address { get; set; }

    [MaxLength(100)]
    [FromForm(Name = "Province")]
    public string? Province { get; set; }

    [MaxLength(100)]
    [FromForm(Name = "City")]
    public string? City { get; set; }

    [MaxLength(100)]
    [FromForm(Name = "District")]
    public string? District { get; set; }

    [MaxLength(10)]
    [FromForm(Name = "Rt")]
    public string? Rt { get; set; }

    [MaxLength(10)]
    [FromForm(Name = "Rw")]
    public string? Rw { get; set; }
}