using IdentityService.Core.Entities;
using IdentityService.Shared.Constants;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Infrastructure.Seeder;

public static class RoleSeeder
{
    public static async Task SeedAsync(DbContext db)
    {
        if (db.Set<Role>() == null)
            throw new InvalidOperationException("Role DbSet not found in DbContext.");

        var roles = new[]
        {
            new Role { Name = RoleNames.Owner, NormalizedName = RoleNames.Owner.ToUpper(), Description = "System Owner", CreDate = DateTime.UtcNow },
            new Role { Name = RoleNames.Admin, NormalizedName = RoleNames.Admin.ToUpper(), Description = "Administrator", CreDate = DateTime.UtcNow },
            new Role { Name = RoleNames.Manager, NormalizedName = RoleNames.Manager.ToUpper(), Description = "Manager", CreDate = DateTime.UtcNow },
            new Role { Name = RoleNames.Cashier, NormalizedName = RoleNames.Cashier.ToUpper(), Description = "Cashier", CreDate = DateTime.UtcNow },
            new Role { Name = RoleNames.Inventory, NormalizedName = RoleNames.Inventory.ToUpper(), Description = "Inventory", CreDate = DateTime.UtcNow },
            new Role { Name = RoleNames.Finance, NormalizedName = RoleNames.Finance.ToUpper(), Description = "Finance", CreDate = DateTime.UtcNow },
            new Role { Name = RoleNames.Kitchen, NormalizedName = RoleNames.Kitchen.ToUpper(), Description = "Kitchen", CreDate = DateTime.UtcNow },
            new Role { Name = RoleNames.Support, NormalizedName = RoleNames.Support.ToUpper(), Description = "Support", CreDate = DateTime.UtcNow },
            new Role { Name = RoleNames.Customer, NormalizedName = RoleNames.Customer.ToUpper(), Description = "Customer", CreDate = DateTime.UtcNow }
        };

        foreach (var role in roles)
        {
            bool exists = await db.Set<Role>()
                .AnyAsync(r => r.NormalizedName == role.NormalizedName);

            if (!exists)
            {
                await db.AddAsync(role);
                await db.SaveChangesAsync();
            }
            
            var newrole = await db.Set<Role>().AsNoTracking().ToListAsync();
        }

        await db.SaveChangesAsync();
    }
}
