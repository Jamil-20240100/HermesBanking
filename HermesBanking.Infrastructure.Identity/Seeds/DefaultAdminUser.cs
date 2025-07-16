using HermesBanking.Core.Domain.Common.Enums;
using HermesBanking.Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HermesBanking.Infrastructure.Identity.Seeds
{
    public static class DefaultAdminUser
    {
        public static async Task SeedAsync(UserManager<AppUser> userManager)
        {
            AppUser user = new()
            {
                Name = "Jane",
                LastName = "Doe",
                Email = "admin@mail.com",
                EmailConfirmed = true,
                UserName = "JaneDoeAdmin",
                UserId = "40230098754",
                IsActive = true,
            };

            if (await userManager.Users.AllAsync(u => u.Id != user.Id))
            {
                var entityUser = await userManager.FindByEmailAsync(user.Email);
                if (entityUser == null)
                {
                    await userManager.CreateAsync(user, "123Pa$$word!");
                    await userManager.AddToRoleAsync(user, Roles.Admin.ToString());
                }
            }

        }
    }
}
