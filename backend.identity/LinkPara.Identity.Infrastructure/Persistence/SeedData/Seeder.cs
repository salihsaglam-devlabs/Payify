using LinkPara.HttpProviders.Vault;
using LinkPara.Identity.Application.Features.Screens.Commands.CreateRoleScreen;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LinkPara.Identity.Infrastructure.Persistence.SeedData;

public class Seeder
{
    private static string Path = "";

    public static async Task EnsureSeedData(IServiceProvider serviceProvider, IVaultClient vaultClient)
    {
        var context = serviceProvider.GetRequiredService<IdentityDbContext>();
        var appContext = serviceProvider.GetRequiredService<ApplicationDbContext>();

        string databaseProvider = vaultClient.GetSecretValue<string>("SharedSecrets", "DatabaseConfiguration", "Provider");

        switch (databaseProvider)
        {
            case "MsSql":
                Path = "../LinkPara.Identity.Infrastructure/Persistence/SeedData/MsSqlData/";
                break;
            default:
                Path = "../LinkPara.Identity.Infrastructure/Persistence/SeedData/PostgreSqlData/";
                break;
        }

        await CreateRoles(context);
        await SecurityPictureSeeder.SeedAsync(appContext);
    }

    private static async Task CreateRoles(IdentityDbContext context)
    {
        if (!context.Roles.Any())
        {
            var sql = await File.ReadAllTextAsync(Path + "Roles.sql");
            await context.Database.ExecuteSqlRawAsync(sql);

            await InsertAdminUser(context);

            await CreateIndividualRoleAndClaims(context);

            await CreateRoleScreens(context);

            await CreateRoleClaims(context);
            
            await context.SaveChangesAsync();
        }
    }
    
    private static async Task InsertAdminUser(IdentityDbContext context)
    {
        var sql = await File.ReadAllTextAsync(Path + "AdminRole.sql");
        await context.Database.ExecuteSqlRawAsync(sql);
    }
    
    private static async Task CreateIndividualRoleAndClaims(IdentityDbContext context)
    {
        var sql = await File.ReadAllTextAsync(Path + "IndividualRole.sql");
        await context.Database.ExecuteSqlRawAsync(sql);
    }
    
    private static async Task CreateRoleScreens(IdentityDbContext context)
    {
        var sql = await File.ReadAllTextAsync(Path + "RoleScreens.sql");
        await context.Database.ExecuteSqlRawAsync(sql);
    }
    
    private static async Task CreateRoleClaims(IdentityDbContext context)
    {
        var sql = await File.ReadAllTextAsync(Path + "RoleClaims.sql");
        await context.Database.ExecuteSqlRawAsync(sql);
    }
}