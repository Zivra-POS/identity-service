using IdentityService.Core.Entities;
using IdentityService.Core.Interfaces.Repositories;
using IdentityService.Infrastructure.Persistence;
using ZivraFramework.EFCore.Repositories;

namespace IdentityService.Infrastructure.Repositories;

public class AccessTokenRepository : GenericRepository<AccessToken>, IAccessTokenRepository
{
    public AccessTokenRepository(IdentityDbContext ctx) : base(ctx)
    {
    }
}