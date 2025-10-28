using IdentityService.Core.Entities;
using IdentityService.Core.Interfaces.Repositories;
using IdentityService.Infrastructure.Persistence;
using ZivraFramework.EFCore.Repositories;

namespace IdentityService.Infrastructure.Repositories;

public class UserSecurityLogRepository : GenericRepository<UserSecurityLog>, IUserSecurityLogRepository
{
    #region Constructor
    public UserSecurityLogRepository(IdentityDbContext ctx) : base(ctx)
    {
    }
    #endregion
}
