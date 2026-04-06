using LinkPara.HttpProviders.Vault;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LinkPara.PF.Infrastructure.Persistence.SeedData;

public class Seeder
{
    private static string Path = "";
    
    public static async Task EnsureSeedData(IServiceProvider serviceProvider, IVaultClient vaultClient)
    {
        var context = serviceProvider.GetRequiredService<PfDbContext>();

        string databaseProvider = vaultClient.GetSecretValue<string>("SharedSecrets", "DatabaseConfiguration", "Provider");

        switch (databaseProvider)
        {
            case "MsSql":
                Path = "../LinkPara.PF.Infrastructure/Persistence/SeedData/MsSqlData/";
                break;
            default:
                Path = "../LinkPara.PF.Infrastructure/Persistence/SeedData/PostgreSqlData/";
                break;
        }
        
        await CreateBanks(context);
        await CreateAcquireBanks(context);
        await CreateBankApiKeys(context);
        await CreateCurrencies(context);
        await CreateMerchantMccList(context);
        await CreateMerchantResponseCodes(context);
        await CreateApiResponseCodes(context);
        await CreateBankResponseCodes(context);
        await CreateNaceCodes(context);
    }
    
    private static async Task CreateBanks(PfDbContext context)
    {
        if (!context.Bank.Any())
        {
            var sql = await File.ReadAllTextAsync(Path + "Banks.sql");
            await context.Database.ExecuteSqlRawAsync(sql);
            await context.SaveChangesAsync();
        }
    }
    
    private static async Task CreateAcquireBanks(PfDbContext context)
    {
        if (!context.AcquireBank.Any())
        {
            var sql = await File.ReadAllTextAsync(Path + "AcquireBanks.sql");
            await context.Database.ExecuteSqlRawAsync(sql);
            await context.SaveChangesAsync();
        }
    }
    
    private static async Task CreateBankApiKeys(PfDbContext context)
    {
        if (!context.BankApiKey.Any())
        {
            var sql = await File.ReadAllTextAsync(Path + "BankApiKeys.sql");
            await context.Database.ExecuteSqlRawAsync(sql);
            await context.SaveChangesAsync();
        }
    }
    
    private static async Task CreateCurrencies(PfDbContext context)
    {
        if (!context.Currency.Any())
        {
            var sql = await File.ReadAllTextAsync(Path + "Currencies.sql");
            await context.Database.ExecuteSqlRawAsync(sql);
            await context.SaveChangesAsync();
        }
    }
    
    private static async Task CreateMerchantMccList(PfDbContext context)
    {
        if (!context.Mcc.Any())
        {
            var sql = await File.ReadAllTextAsync(Path + "MccList.sql");
            await context.Database.ExecuteSqlRawAsync(sql);
            await context.SaveChangesAsync();
        }
    }
    
    private static async Task CreateMerchantResponseCodes(PfDbContext context)
    {
        if (!context.MerchantResponseCode.Any())
        {
            var sql = await File.ReadAllTextAsync(Path + "MerchantResponseCodes.sql");
            await context.Database.ExecuteSqlRawAsync(sql);
            await context.SaveChangesAsync();
        }
    }
    
    private static async Task CreateApiResponseCodes(PfDbContext context)
    {
        if (!context.ApiResponseCode.Any())
        {
            var sql = await File.ReadAllTextAsync(Path + "ApiResponseCodes.sql");
            await context.Database.ExecuteSqlRawAsync(sql);
            await context.SaveChangesAsync();
        }
    }
    
    private static async Task CreateBankResponseCodes(PfDbContext context)
    {
        if (!context.BankResponseCode.Any())
        {
            var sql = await File.ReadAllTextAsync(Path + "BankResponseCodes.sql");
            await context.Database.ExecuteSqlRawAsync(sql);
            await context.SaveChangesAsync();
        }
    }
    
    private static async Task CreateNaceCodes(PfDbContext context)
    {
        if (!context.Nace.Any())
        {
            var sql = await File.ReadAllTextAsync(Path + "NaceCodes.sql");
            await context.Database.ExecuteSqlRawAsync(sql);
            await context.SaveChangesAsync();
        }
    }
}