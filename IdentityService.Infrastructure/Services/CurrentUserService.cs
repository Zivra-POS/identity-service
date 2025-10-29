using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using IdentityService.Core.Interfaces.Services;
using System.Linq;
using System.Collections.Generic;

namespace IdentityService.Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public Guid? UserId
    {
        get
        {
            var id = User?.FindFirstValue(JwtRegisteredClaimNames.Sub)
                     ?? User?.FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(id, out var guid) ? guid : null;
        }
    }

    public string? Username =>
        User?.FindFirstValue(JwtRegisteredClaimNames.UniqueName) ??
        User?.FindFirstValue(ClaimTypes.Name);

    public string? Email =>
        User?.FindFirstValue(JwtRegisteredClaimNames.Email) ??
        User?.FindFirstValue(ClaimTypes.Email);

    public string? FullName =>
        User?.FindFirstValue(JwtRegisteredClaimNames.Name) ??
        User?.FindFirstValue(ClaimTypes.GivenName) ??
        User?.FindFirstValue(ClaimTypes.Name);

    public IEnumerable<string> Roles =>
        (User?.FindAll(ClaimTypes.Role).Select(c => c.Value)
         ?? User?.Claims.Where(c => string.Equals(c.Type, "role", StringComparison.OrdinalIgnoreCase)).Select(c => c.Value)
         ?? Enumerable.Empty<string>()).ToList();

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;
}