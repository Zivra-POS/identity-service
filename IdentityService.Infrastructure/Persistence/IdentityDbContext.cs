using System;
using IdentityService.Core.Entities;
using Microsoft.EntityFrameworkCore;
using ZivraFramework.EFCore.Context;
using IdentityService.Shared.Constants;

namespace IdentityService.Infrastructure.Persistence;

public class IdentityDbContext(DbContextOptions<IdentityDbContext> options)
    : BaseDbContext<IdentityDbContext>(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<UserClaim> UserClaims => Set<UserClaim>();
    public DbSet<RoleClaim> RoleClaims => Set<RoleClaim>();
    public DbSet<UserLogin> UserLogins => Set<UserLogin>();
    public DbSet<AccessToken> AccessTokens => Set<AccessToken>();
    public DbSet<UserToken> UserTokens => Set<UserToken>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<PasswordHistory> PasswordHistories => Set<PasswordHistory>();
    public DbSet<UserSecurityLog> SecurityLogs => Set<UserSecurityLog>();
    public DbSet<UserSession> UserSessions => Set<UserSession>();
    public DbSet<Store> Stores => Set<Store>();
    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<UserBranch> UserBranches => Set<UserBranch>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        // -------------------- User
        b.Entity<User>(e =>
        {
            e.ToTable("user");
            e.Property(x => x.FullName).HasMaxLength(512);
            e.HasIndex(x => x.NormalizedUsername).IsUnique();
            e.HasIndex(x => x.NormalizedEmail);
            // Added index on HashedId
            e.HasIndex(x => x.HashedId).IsUnique();
            e.Property(x => x.Username).IsRequired().HasMaxLength(100);
            e.Property(x => x.NormalizedUsername).IsRequired().HasMaxLength(100);
            e.Property(x => x.Email).IsRequired().HasMaxLength(256);
            e.Property(x => x.NormalizedEmail).IsRequired().HasMaxLength(256);
            e.Property(x => x.PasswordHash).IsRequired().HasMaxLength(512);
            e.Property(x => x.SecurityStamp).IsRequired().HasMaxLength(128);
            e.Property(x => x.ConcurrencyStamp).HasMaxLength(128);
            e.Property(x => x.PhoneNumber).HasMaxLength(32);
            e.Property(x => x.DisplayName).HasMaxLength(256);
            e.Property(x => x.Province).HasMaxLength(100);
            e.Property(x => x.City).HasMaxLength(100);
            e.Property(x => x.District).HasMaxLength(100);
            e.Property(x => x.Rt).HasMaxLength(10).HasColumnName("rt");
            e.Property(x => x.Rw).HasMaxLength(10).HasColumnName("rw");
            e.Property(x => x.CreBy).HasMaxLength(100);
            e.Property(x => x.CreIpAddress).HasMaxLength(64);
            e.Property(x => x.ModBy).HasMaxLength(100);
            e.Property(x => x.ModIpAddress).HasMaxLength(64);
            e.Property(x => x.Address).HasMaxLength(1000);
        });

        // -------------------- Role
        b.Entity<Role>(e =>
        {
            e.ToTable("role");
            e.Property(r => r.Name).IsRequired().HasMaxLength(256);
            e.Property(r => r.NormalizedName).IsRequired().HasMaxLength(256);
            e.HasIndex(r => r.HashedId).IsUnique();
        });
        
        // -------------------- Store
        b.Entity<Store>(e =>
        {
            e.ToTable("store");
            // Added index on HashedId
            e.HasIndex(x => x.HashedId).IsUnique();
            e.Property(x => x.Name).IsRequired().HasMaxLength(256);
            e.Property(x => x.Code).HasMaxLength(100);
            e.Property(x => x.Address).HasMaxLength(512);
            e.Property(x => x.Phone).HasMaxLength(32);

            e.HasMany(x => x.Branches)
             .WithOne(bn => bn.Store)
             .HasForeignKey(bn => bn.StoreId)
             .OnDelete(DeleteBehavior.Cascade);
        });
        
        // -------------------- Branch
        b.Entity<Branch>(e =>
        {
            e.ToTable("branch");
            // Added index on HashedId
            e.HasIndex(x => x.HashedId).IsUnique();
            e.HasIndex(x => x.Code);
            e.Property(x => x.Name).IsRequired().HasMaxLength(256);
            e.Property(x => x.Code).HasMaxLength(100);
            e.Property(x => x.Address).HasMaxLength(1000);
            e.Property(x => x.Province).HasMaxLength(100);
            e.Property(x => x.City).HasMaxLength(100);
            e.Property(x => x.District).HasMaxLength(100);
            e.Property(x => x.Rt).HasMaxLength(10).HasColumnName("rt");
            e.Property(x => x.Rw).HasMaxLength(10).HasColumnName("rw");
            e.Property(x => x.Phone).HasMaxLength(32);

            e.HasOne(x => x.Store)
             .WithMany(s => s.Branches)
             .HasForeignKey(x => x.StoreId)
             .OnDelete(DeleteBehavior.Cascade);
        });
        
        // -------------------- UserBranch (join)
        b.Entity<UserBranch>(e =>
        {
            e.ToTable("user_branch");
            // Added index on HashedId
            e.HasIndex(x => x.HashedId).IsUnique();
            e.HasIndex(x => new { x.UserId, x.BranchId }).IsUnique();
            e.Property(x => x.IsPrimary);

            e.HasOne(x => x.User)
             .WithMany(u => u.UserBranches)
             .HasForeignKey(x => x.UserId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Branch)
             .WithMany(bn => bn.UserBranches)
             .HasForeignKey(x => x.BranchId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // -------------------- UserRole (join)
        b.Entity<UserRole>(e =>
        {
            e.ToTable("user_role");
            e.HasIndex(x => new { x.UserId, x.RoleId }).IsUnique();

            e.HasOne(x => x.User)
             .WithMany(u => u.UserRoles)
             .HasForeignKey(x => x.UserId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Role)
             .WithMany(r => r.UserRoles)
             .HasForeignKey(x => x.RoleId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // -------------------- Claims
        b.Entity<UserClaim>(e =>
        {
            e.ToTable("user_claim");
            // Added index on HashedId
            e.HasIndex(x => x.HashedId).IsUnique();
            e.HasIndex(x => x.UserId);
            e.Property(x => x.ClaimType).IsRequired().HasMaxLength(200);
            e.Property(x => x.ClaimValue).IsRequired().HasMaxLength(2000);

            e.HasOne(x => x.User)
             .WithMany(u => u.Claims)
             .HasForeignKey(x => x.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<RoleClaim>(e =>
        {
            e.ToTable("role_claim");
            // Added index on HashedId
            e.HasIndex(x => x.HashedId).IsUnique();
            e.HasIndex(x => x.RoleId);
            e.Property(x => x.ClaimType).IsRequired().HasMaxLength(200);
            e.Property(x => x.ClaimValue).IsRequired().HasMaxLength(2000);

            e.HasOne(x => x.Role)
             .WithMany(r => r.Claims)
             .HasForeignKey(x => x.RoleId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // -------------------- Logins
        b.Entity<UserLogin>(e =>
        {
            e.ToTable("user_login");
            // Added index on HashedId
            e.HasIndex(x => x.HashedId).IsUnique();
            e.HasIndex(x => new { x.LoginProvider, x.ProviderKey }).IsUnique();
            e.Property(x => x.LoginProvider).IsRequired().HasMaxLength(100);
            e.Property(x => x.ProviderKey).IsRequired().HasMaxLength(256);
            e.Property(x => x.ProviderDisplayName).HasMaxLength(200);

            e.HasOne(x => x.User)
             .WithMany(u => u.Logins)
             .HasForeignKey(x => x.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // -------------------- Tokens
        b.Entity<UserToken>(e =>
        {
            e.ToTable("user_token");
            // Added index on HashedId
            e.HasIndex(x => x.HashedId).IsUnique();
            e.HasIndex(x => new { x.UserId, x.LoginProvider, x.Name }).IsUnique();
            e.Property(x => x.LoginProvider).IsRequired().HasMaxLength(100);
            e.Property(x => x.Name).IsRequired().HasMaxLength(200);
            e.Property(x => x.Value).IsRequired();

            e.HasOne(x => x.User)
             .WithMany(u => u.Tokens)
             .HasForeignKey(x => x.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        });
        
        // -------------------- AccessToken
        b.Entity<AccessToken>(e =>
        {
            e.ToTable("access_token");
            // Added index on HashedId
            e.HasIndex(x => x.HashedId).IsUnique();
            e.HasIndex(x => new { x.UserId, x.Token }).IsUnique();
            e.Property(x => x.Token).IsRequired().HasColumnType("text");
            e.Property(x => x.RevokedByIp).HasMaxLength(64);
        });

        // -------------------- RefreshToken
        b.Entity<RefreshToken>(e =>
        {
            e.ToTable("refresh_token");
            // Added index on HashedId
            e.HasIndex(x => x.HashedId).IsUnique();
            e.HasIndex(x => new { x.UserId, x.Token }).IsUnique();
            e.Property(x => x.Token).IsRequired().HasMaxLength(256);
            e.Property(x => x.DeviceId).HasMaxLength(128);
            e.Property(x => x.RevokedByIp).HasMaxLength(64);
            e.Property(x => x.ReplacedByToken).HasMaxLength(256);

            e.HasOne(x => x.User)
             .WithMany(u => u.RefreshTokens)
             .HasForeignKey(x => x.UserId)
             .OnDelete(DeleteBehavior.Cascade);
            
             e.HasOne(x => x.AccessToken)
                 .WithMany()
                 .HasForeignKey(x => x.AccessTokenId)
                 .OnDelete(DeleteBehavior.Cascade);
        });

        // -------------------- PasswordHistory
        b.Entity<PasswordHistory>(e =>
        {
            e.ToTable("password_history");
            // Added index on HashedId
            e.HasIndex(x => x.HashedId).IsUnique();
            e.HasIndex(x => x.UserId);
            e.Property(x => x.PasswordHash).IsRequired();

            e.HasOne(x => x.User)
             .WithMany(u => u.PasswordHistories)
             .HasForeignKey(x => x.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // -------------------- SecurityLog
        b.Entity<UserSecurityLog>(e =>
        {
            e.ToTable("user_security_log");
            // Added index on HashedId
            e.HasIndex(x => x.HashedId).IsUnique();
            e.HasIndex(x => x.UserId);
            e.Property(x => x.Action).IsRequired().HasMaxLength(100);
            e.Property(x => x.Description).HasMaxLength(2000);
            e.Property(x => x.IpAddress).HasMaxLength(64);
            e.Property(x => x.UserAgent).HasMaxLength(512);

            e.HasOne(x => x.User)
             .WithMany(u => u.SecurityLogs)
             .HasForeignKey(x => x.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // -------------------- UserSession
        b.Entity<UserSession>(e =>
        {
            e.ToTable("user_session");
            // Added index on HashedId
            e.HasIndex(x => x.HashedId).IsUnique();
            e.HasIndex(x => x.UserId);
            e.HasIndex(x => x.SessionId).IsUnique();
            e.Property(x => x.SessionId).IsRequired().HasMaxLength(128);
            e.Property(x => x.DeviceId).HasMaxLength(128);
            e.Property(x => x.DeviceName).HasMaxLength(256);
            e.Property(x => x.IpAddress).HasMaxLength(64);
            e.Property(x => x.UserAgent).HasMaxLength(512);

            e.HasOne(x => x.User)
             .WithMany(u => u.UserSessions)
             .HasForeignKey(x => x.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        });
    }
}