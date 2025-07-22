using HermesBanking.Core.Domain.Common.Enums;
using Microsoft.AspNetCore.Identity;

namespace HermesBanking.Infrastructure.Identity.Seeds
{
    public static class DefaultRoles
    {
        public static async Task SeedAsync(RoleManager<IdentityRole> roleManager)
        {
            foreach (var roleName in new[] { Roles.Admin.ToString(), Roles.Commerce.ToString(), Roles.Client.ToString(), Roles.Cashier.ToString() })
            {
               if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
        }
    }
}
