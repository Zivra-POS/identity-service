using System.Collections.Generic;

namespace IdentityService.Core.Interfaces.Services;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? Username { get; }
    string? Email { get; }
    string? FullName { get; }
    // Added Roles so callers can read role claims
    IEnumerable<string> Roles { get; }
    Guid? StoreId { get; }
    bool IsAuthenticated { get; }
}