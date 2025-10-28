using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ZivraFramework.Core.Interfaces;

namespace IdentityService.Core.Entities;

public class Store : IBaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [MaxLength(256)]
    public string Name { get; set; } = default!;

    [MaxLength(100)]
    public string? Code { get; set; }

    [MaxLength(512)]
    public string? Address { get; set; }

    [MaxLength(32)]
    public string? Phone { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreDate { get; set; }
    public string? CreBy { get; set; }
    public string? CreIpAddress { get; set; }
    public DateTime? ModDate { get; set; }
    public string? ModBy { get; set; }
    public string? ModIpAddress { get; set; }

    public ICollection<IdentityService.Core.Entities.Branch> Branches { get; set; } = new List<IdentityService.Core.Entities.Branch>();
    public ICollection<User> Users { get; set; } = new List<User>();
}
