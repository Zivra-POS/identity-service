namespace IdentityService.Shared.Constants;

public static class UserSecurityActions
{
    public const string LoginSuccess = "Login Berhasil";
    public const string LoginFailed = "Login Gagal";
    public const string Logout = "Logout";
    public const string SendEmailVerification = "Permintaan Verifikasi Email";
    public const string EmailVerified = "Email Terverifikasi";
    public const string PasswordChanged = "Password Diubah";
    public const string PasswordResetRequested = "Permintaan Reset Password";
    public const string PasswordResetCompleted = "Reset Password Berhasil";
    public const string TwoFactorEnabled = "Two Factor Diaktifkan";
    public const string TwoFactorDisabled = "Two Factor Dinonaktifkan";
    public const string AccountCreated = "Akun Dibuat";
    public const string AccountUpdated = "Akun Diperbarui";
    public const string AccountDeactivated = "Akun Dinonaktifkan";
    public const string AccountReactivated = "Akun Diaktifkan";
    public const string ProfileUpdated = "Profil Diperbarui";
    public const string RoleChanged = "Peran Diubah";
    public const string AccessTokenIssued = "Token Akses Dibuat";
    public const string RefreshTokenIssued = "Token Refresh Dibuat";
    public const string TokenRevoked = "Token Dicabut";
    public const string FailedAccessAttempt = "Percobaan Login Gagal";
    public const string LockoutEnabled = "Penguncian Diaktifkan";
    public const string LockoutDisabled = "Penguncian Dinonaktifkan";
    public const string SessionTerminated = "Sesi Dihentikan";
    public const string IpBlocked = "IP Diblokir";
    public const string SuspiciousActivityDetected = "Aktivitas Mencurigakan Terdeteksi";
}