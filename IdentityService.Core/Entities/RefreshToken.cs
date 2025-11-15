using System;
using ZivraFramework.Core.Models;

namespace IdentityService.Core.Entities;

public class RefreshToken : BaseEntity
{
    public Guid AccessTokenId { get; set; }
    public Guid UserId { get; set; }
    public string Token { get; set; } = default!;              
    public DateTime Expires { get; set; }
    public DateTime? Revoked { get; set; }
    public string? RevokedByIp { get; set; }
    public string? ReplacedByToken { get; set; }               
    public string? DeviceId { get; set; }                   
    public bool IsActive => Revoked == null && DateTime.UtcNow < Expires;

    public User? User { get; set; }
    public AccessToken? AccessToken { get; set; }
}
