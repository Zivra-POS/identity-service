using IdentityService.Core.Entities;

namespace IdentityService.Core.Interfaces.Services;

public interface ITokenService
{
    Task<AccessToken> GenerateJwtToken(User user, IEnumerable<string?> roles);
}