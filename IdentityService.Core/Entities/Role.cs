using System;
using System.Collections.Generic;
using ZivraFramework.Core.Interfaces;

namespace IdentityService.Core.Entities;

public class Role : IBaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = default!;
    public string NormalizedName { get; set; } = default!;
    public string? Description { get; set; }
    public string? ConcurrencyStamp { get; set; } = Guid.NewGuid().ToString("N");

    public DateTime CreDate { get; set; }
    public string? CreBy { get; set; }
    public string? CreIpAddress { get; set; }
    public DateTime? ModDate { get; set; }
    public string? ModBy { get; set; }
    public string? ModIpAddress { get; set; }

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<RoleClaim> Claims { get; set; } = new List<RoleClaim>();
}
