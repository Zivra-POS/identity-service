namespace IdentityService.Shared.Constants;

public static class LockoutSettings
{
    public const int DefaultMaxFailedAttempts = 5;
    public const int DefaultLockoutMinutes = 30;
    public const string MaxFailedAttemptsKey = "LockoutSettings:MaxFailedAttempts";
    public const string LockoutMinutesKey = "LockoutSettings:LockoutMinutes";
}
