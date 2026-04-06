using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Infrastructure.Persistence;
using LinkPara.HttpProviders.BusinessParameter;
using LinkPara.HttpProviders.DbProvider;
using LinkPara.HttpProviders.Emoney.Enums;
using LinkPara.HttpProviders.Vault;
using LinkPara.SharedModels.BusModels.Commands.Notification;
using LinkPara.SharedModels.Persistence;
using LinkPara.SystemUser;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Transactions;

namespace LinkPara.Emoney.Infrastructure.Services;

public class VirtualIbanService : IVirtualIbanService
{
    private const int MaxLimitPercent = 90;

    private readonly ILogger<VirtualIbanService> _logger;
    private readonly IEmailSender _emailSender;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IParameterService _parameterService;
    private readonly IApplicationUserService _applicationUserService;
    private readonly IVaultClient _vaultClient;
    private readonly IDatabaseProviderService _databaseProviderService;

    public VirtualIbanService(ILogger<VirtualIbanService> logger,
        IEmailSender emailSender,
        IServiceScopeFactory scopeFactory,
        IParameterService parameterService,
        IApplicationUserService applicationUserService,
        IVaultClient vaultClient,
        IDatabaseProviderService databaseProviderService)
    {
        _logger = logger;
        _emailSender = emailSender;
        _scopeFactory = scopeFactory;
        _parameterService = parameterService;
        _applicationUserService = applicationUserService;
        _vaultClient = vaultClient;
        _databaseProviderService = databaseProviderService;
    }

    public async Task AssignToAccountAsync(Guid accountId)
    {
        using var scope = _scopeFactory.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<EmoneyDbContext>();
        var strategy = dbContext.Database.CreateExecutionStrategy();

        var sql = string.Empty;

        var databaseProvider = await _databaseProviderService.GetProviderAsync();
        switch (databaseProvider)
        {
            case "MsSql":
                {
                    sql = "SELECT TOP 1 * FROM Core.VirtualIban WITH (ROWLOCK, UPDLOCK) WHERE Available = 1 AND RecordStatus = 'Active'";
                    break;
                }
            default:
                {
                    sql = "select * from core.virtual_iban where available = true and record_status = 'Active' FOR UPDATE limit 1";
                    break;
                }
        }

        var virtualIban = await dbContext.VirtualIban
            .FromSqlRaw(sql)
            .FirstOrDefaultAsync();

        if (virtualIban is null)
        {
            return;
        }

        await strategy.ExecuteAsync(async () =>
        {
            try
            {
                using var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

                var account = await dbContext.Account.FirstOrDefaultAsync(s => s.Id == accountId);

                account.VirtualIban = virtualIban.Iban;
                account.UpdateDate = DateTime.Now;
                account.LastModifiedBy = _applicationUserService.ApplicationUserId.ToString();

                virtualIban.Available = false;
                virtualIban.UpdateDate = DateTime.Now;
                virtualIban.LastModifiedBy = _applicationUserService.ApplicationUserId.ToString();

                dbContext.Update(virtualIban);
                dbContext.Update(account);

                await dbContext.SaveChangesAsync();

                transactionScope.Complete();
            }
            catch (Exception exception)
            {
                _logger.LogError($"Virtual Iban Assignment Error : {exception}");
            }
        });
    }

    public async Task AssignToAccountsAsync()
    {
        var serviceIsActive = _vaultClient.GetSecretValue<bool>("SharedSecrets", "ServiceState", "VirtualIbanAssignment");

        if (!serviceIsActive)
        {
            return;
        }

        using var scope = _scopeFactory.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<EmoneyDbContext>();

        var minLevel = await GetMinKycLevelToIbanAssignment();

        var accounts = dbContext.Account
            .Where(s => (int)s.AccountKycLevel >= minLevel && string.IsNullOrWhiteSpace(s.VirtualIban));

        foreach (var account in accounts)
        {
            await AssignToAccountAsync(account.Id);
        }
    }

    public async Task CheckAvailableCount()
    {
        var serviceIsActive = _vaultClient.GetSecretValue<bool>("SharedSecrets", "ServiceState", "VirtualIbanAssignment");

        if (!serviceIsActive)
        {
            return;
        }

        using var scope = _scopeFactory.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<EmoneyDbContext>();

        var ibanList = dbContext.VirtualIban
            .Where(s => s.RecordStatus == RecordStatus.Active)
            .ToList();

        if (!ibanList.Any())
        {
            return;
        }

        var totalCount = ibanList.Count;
        var usedIbanCount = ibanList.Count(s => !s.Available);
        var criticalLimit = totalCount * (MaxLimitPercent / 100m);

        if (usedIbanCount >= criticalLimit)
        {
            var emailList = await GetEmailAddressesAsync();

            foreach (var item in emailList)
            {
                var emailRequest = new SendEmail
                {
                    ToEmail = item,
                    DynamicTemplateData = new Dictionary<string, string>
                    {
                        { "subject", "Sanal IBAN Kullanımı Limit Uyarısı!" },
                        { "content", $"Kalan Sanal IBAN Sayısı : { totalCount - usedIbanCount }"}
                    }
                };

                await _emailSender.SendEmailAsync(emailRequest);
            }
        }
    }

    public async Task<bool> CheckKycLevelToIbanAssignmentAsync(AccountKycLevel kycLevel)
    {
        try
        {
            var serviceIsActive = _vaultClient.GetSecretValue<bool>("SharedSecrets", "ServiceState", "VirtualIbanAssignment");

            if (serviceIsActive)
            {
                var parameter = await _parameterService.GetParameterAsync("VirtualIbanParameters", "AccountKycLevel");

                return (int)kycLevel >= Convert.ToInt32(parameter.ParameterValue);
            }

            return false;
        }
        catch (Exception exception)
        {
            _logger.LogError($"GetParameterAsync Error : {exception} ");

            return false;
        }
    }

    private async Task<int> GetMinKycLevelToIbanAssignment()
    {
        try
        {
            var parameter = await _parameterService.GetParameterAsync("VirtualIbanParameters", "AccountKycLevel");

            return Convert.ToInt32(parameter.ParameterValue);
        }
        catch (Exception exception)
        {
            _logger.LogError($"GetParameterAsync Error : {exception} ");

            return 0;
        }
    }

    private async Task<string[]> GetEmailAddressesAsync()
    {
        try
        {
            var parameter = await _parameterService.GetParameterAsync("VirtualIbanParameters", "MailsForAlert");
            if (!string.IsNullOrEmpty(parameter.ParameterValue))
            {
                return parameter.ParameterValue.Split(',');
            }
        }
        catch (Exception exception)
        {
            _logger.LogError($"GetParameterAsync Error : {exception} ");
        }

        return Array.Empty<string>();

    }

}
