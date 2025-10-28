using IdentityService.Core.Entities;
using ZivraFramework.Core.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace IdentityService.Core.Interfaces.Repositories;

public interface IUserRepository : IGenericRepository<User>
{
    Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default);
    Task<bool> ExistsUsernameAsync(string username, CancellationToken ct = default);
}