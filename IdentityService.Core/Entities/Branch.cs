using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ZivraFramework.Core.Models;

namespace IdentityService.Core.Entities;

public class Branch : BaseEntity
{
    public Guid StoreId { get; set; }

    [MaxLength(256)]
    public string Name { get; set; } = default!;

    [MaxLength(100)]
    public string? Code { get; set; }

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

    [MaxLength(32)]
    public string? Phone { get; set; }

    public bool IsActive { get; set; } = true;

    public Store? Store { get; set; }
    public ICollection<UserBranch> UserBranches { get; set; } = new List<UserBranch>();
}
