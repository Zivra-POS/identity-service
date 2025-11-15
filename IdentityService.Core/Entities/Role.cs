using System;
using System.Collections.Generic;
using ZivraFramework.Core.Models;

namespace IdentityService.Core.Entities;

public class Role : BaseEntity
{
    public string Name { get; set; } = default!;
    public string NormalizedName { get; set; } = default!;
    public string? Description { get; set; }
    public string? ConcurrencyStamp { get; set; } = Guid.NewGuid().ToString("N");

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<RoleClaim> Claims { get; set; } = new List<RoleClaim>();
}
