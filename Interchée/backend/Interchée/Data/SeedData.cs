using Interchée.Config;
using Interchée.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Interchée.Data
{
    public static class SeedData
    {
        public static async Task RunAsync(IServiceProvider sp)
        {
            var db = sp.GetRequiredService<AppDbContext>();
            await db.Database.MigrateAsync();

            // Ensure Identity roles exist
            var roleMgr = sp.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
            foreach (var r in new[] { Roles.Admin, Roles.HR, Roles.Supervisor, Roles.Attache, Roles.Intern })
            {
                if (!await roleMgr.RoleExistsAsync(r))
                    await roleMgr.CreateAsync(new IdentityRole<Guid>(r));
            }

            // Seed two departments if none
            if (!await db.Departments.AnyAsync())
            {
                db.Departments.AddRange(
                    new Department { Name = "IT", Code = "IT" },
                    new Department { Name = "Finance", Code = "FIN" }
                );
                await db.SaveChangesAsync();
            }

            // Seed one admin user if missing
            var userMgr = sp.GetRequiredService<UserManager<AppUser>>();
            var adminEmail = "admin@demo.local";
            var admin = await userMgr.Users.FirstOrDefaultAsync(u => u.Email == adminEmail);

            if (admin == null)
            {
                admin = new AppUser
                {
                    Id = Guid.NewGuid(),
                    UserName = "admin",
                    Email = adminEmail,
                    EmailConfirmed = true,
                    FirstName = "System",
                    LastName = "Admin"
                };
                // dev password — change in real deployments
                await userMgr.CreateAsync(admin, "P@ssw0rd!");

                // Global Identity role
                await userMgr.AddToRoleAsync(admin, Roles.Admin);

                // Department-scoped Admin role (IT by default)
                var itDeptId = await db.Departments.Where(d => d.Name == "IT").Select(d => d.Id).FirstAsync();
                db.DepartmentRoleAssignments.Add(new DepartmentRoleAssignment
                {
                    UserId = admin.Id,
                    DepartmentId = itDeptId,
                    RoleName = Roles.Admin
                });
                await db.SaveChangesAsync();
            }
        }
    }
}