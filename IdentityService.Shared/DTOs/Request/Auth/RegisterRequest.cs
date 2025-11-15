using System.ComponentModel.DataAnnotations;
using ZivraFramework.Core.Models;

namespace IdentityService.Shared.DTOs.Request.Auth;

public class RegisterRequest : BaseDto
{
    public required string FullName { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public Guid[]? RoleIDs { get; set; }

    [MaxLength(1000)]
    public string? Address { get; set; }

    [MaxLength(100)]
    public string? Province { get; set; }

    [MaxLength(100)]
    public string? City { get; set; }

    [MaxLength(100)]
    public string? District { get; set; }

    [MaxLength(10)]
    public string? Rt { get; set; }

    [MaxLength(10)]
    public string? Rw { get; set; }
}