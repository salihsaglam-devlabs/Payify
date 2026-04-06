using LinkPara.HttpProviders.Identity;
using LinkPara.HttpProviders.Vault;
using Microsoft.Extensions.DependencyInjection;

namespace LinkPara.SystemUser
{
    public static class ApplicationUserConfiguration
    {
        public static async Task<IApplicationUserService> ConfigureApplicationUser(this IServiceCollection services, 
            string applicationName, IVaultClient vaultClient = null)
        {
            var username = vaultClient
                .GetSecretValue<string>($"{applicationName}Secrets", "AppUser", "Username");
            
            var password = vaultClient
                .GetSecretValue<string>($"{applicationName}Secrets", "AppUser", "Password");
          
            var userService = services.BuildServiceProvider().GetService<IUserService>();
            var service = new ApplicationUserService(userService);

            service.ApplicationUserId =  await service.ConfigureApplicationUserAsync(username, password);
            service.Token =  await service.ConfigureApplicationLoginAsync(username, password);

            return service;
        }
    }
}
