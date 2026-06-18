using ForumApp.Models;
using Microsoft.AspNetCore.Identity;

namespace ForumApp.Data;

/// <summary>
/// Статичен клас, отговорен за инициализиране на базата данни с първоначални (seed) данни.
/// Създава необходимите роли за системата за оторизация и администраторски акаунт
/// при първоначално стартиране на приложението.
/// </summary>
/// <remarks>
/// Този клас е от съществено значение за правилното функциониране на системата,
/// тъй като дефинира ролевата йерархия: Admin, Moderator и User.
/// Администраторската роля има пълен достъп до управлението на потребители,
/// докато модераторите отговарят за прегледа на коментари, маркирани от ML модела.
/// </remarks>
public static class DbSeeder
{
    /// <summary>
    /// Асинхронно създава системните роли и администраторски акаунт, ако не съществуват.
    /// </summary>
    /// <param name="serviceProvider">
    /// Доставчик на услуги, използван за получаване на <see cref="RoleManager{TRole}"/>
    /// и <see cref="UserManager{TUser}"/> инстанции.
    /// </param>
    /// <returns>
    /// Задача, представляваща асинхронната операция по инициализиране на данните.
    /// </returns>
    /// <remarks>
    /// Методът създава три роли: Admin, Moderator и User.
    /// Ако администраторският акаунт не съществува, създава се с предварително
    /// дефинирани данни за достъп (admin@forum.local / Admin@123456).
    /// Този метод е идемпотентен - безопасно е да се извиква многократно.
    /// </remarks>
    public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        string[] roleNames = { "Admin", "Moderator", "User" };

        foreach (var roleName in roleNames)
        {
            var roleExist = await roleManager.RoleExistsAsync(roleName);
            if (!roleExist)
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        string adminEmail = "admin@forum.local";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                IsActive = true
            };

            var createPowerUser = await userManager.CreateAsync(adminUser, "Admin@123456");
            
            if (createPowerUser.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
    }
}