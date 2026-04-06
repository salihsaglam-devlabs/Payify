using LinkPara.Identity.Domain.Entities;
using LinkPara.Identity.Domain.Enums;
using LinkPara.SharedModels.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LinkPara.Identity.Infrastructure;

public static class DataSeeder
{
    public static async Task InitializeAsync(IApplicationBuilder app,
        IConfiguration configuration)
    {
        var scope = app.ApplicationServices.CreateScope();
        var roleManager = scope.ServiceProvider.GetService<RoleManager<Role>>();
        var userManager = scope.ServiceProvider.GetService<UserManager<User>>();

        await SeedAsync(scope.ServiceProvider, configuration, roleManager, userManager);
    }

    private static async Task SeedAsync(IServiceProvider serviceProvider,
        IConfiguration configuration,
        RoleManager<Role> roleManager,
        UserManager<User> userManager)
    {
        using (var scope = serviceProvider.CreateScope())
        {
            var defaultStaticRoles = new Dictionary<string, string>(){
                { "GeneralDirector", "0000000001" },
                { "ITManager", "0000000002" }};

            var defaultPassword = configuration.GetValue<string>("DefaultDatas:DefaultUsersPassword");
            foreach (var defaultStaticRole in defaultStaticRoles)
            {
                if (!await roleManager.RoleExistsAsync(defaultStaticRole.Key))
                {
                    await roleManager.CreateAsync(new Role()
                    {
                        Name = defaultStaticRole.Key,
                        RoleScope = RoleScope.Internal,
                        CreateDate = DateTime.Now,
                        UpdateDate = DateTime.Now,
                        RecordStatus = RecordStatus.Active
                    });
                }

                if ((await userManager.GetUsersInRoleAsync(defaultStaticRole.Key)).Count == 0)
                {
                    var seedUserBasedRole = new User()
                    {
                        FirstName = "ApplicationSeedUser",
                        LastName = $"As{defaultStaticRole.Key}Role",
                        Email = $"SeedUserAs{defaultStaticRole.Key}@linktera.com",
                        UserName = defaultStaticRole.Value,
                        PhoneNumber = defaultStaticRole.Value.Substring(3),
                        EmailConfirmed = true,
                        PhoneNumberConfirmed = true,
                        UserType = UserType.Internal,
                        UserStatus = UserStatus.Active,
                        RecordStatus = RecordStatus.Active,
                        PasswordModifiedDate = DateTime.Now,
                        CreateDate = DateTime.Now,
                        UpdateDate = DateTime.Now
                    };

                    var result = await userManager.CreateAsync(seedUserBasedRole, defaultPassword);

                    result = result.Succeeded ? await userManager.AddToRoleAsync(seedUserBasedRole, defaultStaticRole.Key) : result;
                }
            }
        }
    }
}