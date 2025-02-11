using ExpensesPlanner.Models;
using Microsoft.AspNetCore.Identity;

namespace ExpensesPlanner.Data
{
    public class Seed
    {
        public static async Task SeedUsersAndRolesAsync(IApplicationBuilder applicationBuilder)
        {
            using var serviceScope = applicationBuilder.ApplicationServices.CreateScope();
            var roleManager = serviceScope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Verify if exists a UserRole "Admin" and "User". Otherwise, create those Roles
            if (!await roleManager.RoleExistsAsync(UserRoles.Admin))
                await roleManager.CreateAsync(new IdentityRole(UserRoles.Admin));
            if (!await roleManager.RoleExistsAsync(UserRoles.User))
                await roleManager.CreateAsync(new IdentityRole(UserRoles.User));

            // Create Users Data to seed
            var userManager = serviceScope.ServiceProvider.GetRequiredService<UserManager<User>>();

            // Create Admin User
            string adminEmail = "tiagofilipe.lanca@gmail.com";

            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                var newAdmin = new User()
                {
                    FirstName = "Tiago",
                    LastName = "Lança",
                    Phone = "938646578",
                    Address = "Rua A.",
                    City = "Seixal",
                    UserName = "tiagofilipe",
                    Email = adminEmail,
                    EmailConfirmed = true,
                };
                var result = await userManager.CreateAsync(newAdmin, "Tiago@1234");
                if (result.Succeeded)
                {
                    Console.WriteLine("Utilizador admin criado com sucesso!\n");
                    await userManager.AddToRoleAsync(newAdmin, UserRoles.Admin);
                }
                else
                {
                    foreach(var error in result.Errors)
                        Console.WriteLine($"Erro ao criar o admin: {error.Description}");
                }
                
            }

            string normalUserEmail = "tiago-diogo1@hotmail.com";

            var normalUser = await userManager.FindByEmailAsync(normalUserEmail);
            if (normalUser == null)
            {
                var newUser = new User()
                {
                    FirstName = "Diogo",
                    LastName = "Lança",
                    Phone = "93123456",
                    Address = "Rua B.",
                    City = "Seixal",
                    UserName = "diogofilipe",
                    Email = normalUserEmail,
                    EmailConfirmed = true,
                };
                var resultUser = await userManager.CreateAsync(newUser, "Diogo@1234");
                if (resultUser.Succeeded) 
                {
                    Console.WriteLine("Utilizador criado com sucesso.\n");
                    await userManager.AddToRoleAsync(newUser, UserRoles.User);
                }
                else
                {
                    foreach(var error in resultUser.Errors)
                        Console.WriteLine($"Erro ao criar user: {error.Description}");
                }
                
            }
        }
    }
}
