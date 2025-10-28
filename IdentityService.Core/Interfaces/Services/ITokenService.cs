using IdentityService.Core.Entities;

namespace IdentityService.Core.Interfaces.Services;

public interface ITokenService
{
    string GenerateJwtToken(User user, IEnumerable<string?> roles);
}