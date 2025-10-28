using IdentityService.Core.Entities;
using IdentityService.Core.Interfaces.Repositories;
using IdentityService.Infrastructure.Persistence;
using ZivraFramework.EFCore.Repositories;

namespace IdentityService.Infrastructure.Repositories;

public class UserLoginRepository : GenericRepository<UserLogin>, IUserLoginRepository
{
    #region Constructor
    public UserLoginRepository(IdentityDbContext ctx) : base(ctx)
    {
    }
    #endregion
}
