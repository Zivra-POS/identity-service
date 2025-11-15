using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using IdentityService.Shared.Constants;
using ZivraFramework.Core.Models;

namespace IdentityService.Core.Entities;

public class Store : BaseEntity
{
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
    public bool IsActive { get; set; }
    public CostingMethod CostingMethod { get; set; } = CostingMethod.FIFO;

    public ICollection<Branch> Branches { get; set; } = new List<Branch>();
    public ICollection<User> Users { get; set; } = new List<User>();
}
