using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ZivraFramework.Core.Interfaces;

namespace IdentityService.Core.Entities;

public class User : IBaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string? ProfileUrl { get; set; }
    [MaxLength(50)]
    public Guid? StoreId { get; set; }
    [MaxLength(50)]
    public Guid? OwnerId { get; set; }
    public string? FullName { get; set; }

    [MaxLength(100)]
    public string Username { get; set; } = default!;
    [MaxLength(100)]
    public string NormalizedUsername { get; set; } = default!;
    [MaxLength(256)]
    public string Email { get; set; } = default!;
    [MaxLength(256)]
    public string NormalizedEmail { get; set; } = default!;
    public bool EmailConfirmed { get; set; }

    [MaxLength(512)]
    public string PasswordHash { get; set; } = default!;
    [MaxLength(128)]
    public string SecurityStamp { get; set; } = Guid.NewGuid().ToString("N");
    [MaxLength(128)]
    public string? ConcurrencyStamp { get; set; } = Guid.NewGuid().ToString("N");

    [MaxLength(32)]
    public string? PhoneNumber { get; set; }
    public bool PhoneNumberConfirmed { get; set; }
    public bool TwoFactorEnabled { get; set; }

    public DateTimeOffset? LockoutEnd { get; set; }
    public bool LockoutEnabled { get; set; } = true;
    public int AccessFailedCount { get; set; }

    [MaxLength(256)]
    public string? DisplayName { get; set; }

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

    public bool IsActive { get; set; } = true;

    public DateTime CreDate { get; set; }
    [MaxLength(100)]
    public string? CreBy { get; set; }
    [MaxLength(64)]
    public string? CreIpAddress { get; set; }
    public DateTime? ModDate { get; set; }
    [MaxLength(100)]
    public string? ModBy { get; set; }
    [MaxLength(64)]
    public string? ModIpAddress { get; set; }

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<UserClaim> Claims { get; set; } = new List<UserClaim>();
    public ICollection<UserLogin> Logins { get; set; } = new List<UserLogin>();
    public ICollection<UserToken> Tokens { get; set; } = new List<UserToken>();
    public ICollection<UserToken> UserTokens { get; set; } = new List<UserToken>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public ICollection<PasswordHistory> PasswordHistories { get; set; } = new List<PasswordHistory>();
    public ICollection<UserSecurityLog> SecurityLogs { get; set; } = new List<UserSecurityLog>();
    public ICollection<UserSession> UserSessions { get; set; } = new List<UserSession>();
    public ICollection<UserBranch> UserBranches { get; set; } = new List<UserBranch>();
}