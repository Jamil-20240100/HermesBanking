using HermesBanking.Core.Domain.Common.Enums;
using HermesBanking.Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;

public static class DefaultAdminUser
{
    public static async Task SeedAsync(UserManager<AppUser> userManager)
    {
        var email = "admin@mail.com";
        var user = await userManager.FindByEmailAsync(email);

        if (user == null)
        {
            AppUser admin = new()
            {
                Name = "Jane",
                LastName = "Doe",
                Email = email,
                EmailConfirmed = false,
                UserName = "JaneDoeAdmin",
                UserId = "40230098754",
                IsActive = true
            };

            await userManager.CreateAsync(admin, "123Pa$$word!");
            await userManager.AddToRoleAsync(admin, Roles.Admin.ToString());

            var token = await userManager.GenerateEmailConfirmationTokenAsync(admin);
            Console.WriteLine($"[ADMIN] Token de confirmación: {token}");
        }
    }
}
