using HermesBanking.Core.Domain.Common.Enums;
using HermesBanking.Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace HermesBanking.Infrastructure.Identity.Seeds
{
    public static class DefaultCommerceUser
    {
        public static async Task SeedAsync(UserManager<AppUser> userManager)
        {
            var email = "comercio@mail.com";
            var user = await userManager.FindByEmailAsync(email);

            if (user == null)
            {
                AppUser comercio = new()
                {
                    Name = "Carlos",
                    LastName = "Empresa",
                    Email = email,
                    EmailConfirmed = false,
                    UserName = "ComercioUser",
                    UserId = "40250012345",
                    IsActive = true
                };

                await userManager.CreateAsync(comercio, "123Pa$$word!");
                await userManager.AddToRoleAsync(comercio, Roles.Commerce.ToString());

                var token = await userManager.GenerateEmailConfirmationTokenAsync(comercio);
                Console.WriteLine($"📧 [COMERCIO] Token de confirmación:\n{token}");
            }
        }
    }
}
