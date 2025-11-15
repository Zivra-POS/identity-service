using ZivraFramework.Core.Models;

namespace IdentityService.Core.Entities;

public class UserBranch : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid BranchId { get; set; }

    public bool IsPrimary { get; set; }

    public User? User { get; set; }
    public Branch? Branch { get; set; }
}
