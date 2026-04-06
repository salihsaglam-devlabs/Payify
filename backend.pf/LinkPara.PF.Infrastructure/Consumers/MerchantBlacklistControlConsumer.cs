using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.HttpProviders.BusinessParameter;
using LinkPara.HttpProviders.Fraud;
using LinkPara.HttpProviders.Fraud.Models;
using LinkPara.HttpProviders.Fraud.Models.Enums;
using LinkPara.HttpProviders.Vault;
using LinkPara.PF.Domain.Entities;
using LinkPara.SharedModels.Boa.Enums;
using LinkPara.SharedModels.BusModels.Commands.PF;
using LinkPara.SharedModels.Notification.NotificationModels.PF;
using LinkPara.SharedModels.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LinkPara.PF.Infrastructure.Consumers;

public class MerchantBlacklistControlConsumer : IConsumer<MerchantBlacklistControl>
{
    private readonly IGenericRepository<Merchant> _repository;
    private readonly ILogger<MerchantBlacklistControlConsumer> _logger;
    private readonly ISearchService _searchService;
    private readonly IAuditLogService _auditLogService;
    private readonly IParameterService _parameterService;
    private readonly IVaultClient _vaultClient;
    private readonly IBus _bus;
    public MerchantBlacklistControlConsumer(IGenericRepository<Merchant> repository,
        ILogger<MerchantBlacklistControlConsumer> logger,
        ISearchService searchService,
        IParameterService parameterService,
        IAuditLogService auditLogService,
        IVaultClient vaultClient,
        IBus bus)
    {
        _repository = repository;
        _logger = logger;
        _searchService = searchService;
        _parameterService = parameterService;
        _auditLogService = auditLogService;
        _vaultClient = vaultClient;
        _bus = bus;
    }
    public async Task Consume(ConsumeContext<MerchantBlacklistControl> context)
    {
        try
        {
            var IsBlacklistCheckEnabled =
                      _vaultClient.GetSecretValue<bool>("SharedSecrets", "ServiceState", "BlacklistEnabled");

            if (!IsBlacklistCheckEnabled)
            {
                return;
            }

            var merchant = await _repository
            .GetAll()
            .Include(s => s.Customer)
            .ThenInclude(b => b.AuthorizedPerson)
            .FirstOrDefaultAsync(b => b.Id == context.Message.MerchantId);

            if (merchant is not null)
            {

                var matchRate = await _parameterService.GetParameterAsync("FraudParameters", "MatchRate");
                var customerName = $"{merchant.Customer.AuthorizedPerson.Name} {merchant.Customer.AuthorizedPerson.Surname}";

                SearchByNameRequest searchRequest = new()
                {
                    Name = customerName,
                    BirthYear = merchant.Customer.AuthorizedPerson.BirthDate.Year.ToString(),
                    SearchType = SearchType.Corporate,
                    FraudChannelType = FraudChannelType.Backoffice
                };

                var blackListControl = await _searchService.GetSearchByName(searchRequest);

                if ((blackListControl.MatchStatus == MatchStatus.PotentialMatch || blackListControl.MatchStatus == MatchStatus.TruePositiveReject) && blackListControl.MatchRate
                                                 >= Convert.ToInt32(matchRate.ParameterValue))
                {
                    await UpdateMerchantAsync(merchant, customerName);
                }
                else
                {
                    SearchByNameRequest searchTitleRequest = new()
                    {
                        Name = merchant.Customer.CommercialTitle,
                        SearchType = SearchType.Corporate,
                        FraudChannelType = FraudChannelType.Backoffice
                    };

                    var blackListTitleControl = await _searchService.GetSearchByName(searchTitleRequest);

                    if ((blackListTitleControl.MatchStatus == MatchStatus.PotentialMatch || blackListTitleControl.MatchStatus == MatchStatus.TruePositiveReject) && blackListTitleControl.MatchRate >= Convert.ToInt32(matchRate.ParameterValue))
                    {
                        await UpdateMerchantAsync(merchant, customerName);
                    }
                }
            }
        }
        catch (Exception exception)
        {
            _logger.LogError($"MerchantBlacklistControlConsumer Error : {exception}");
        }
    }
    private async Task UpdateMerchantAsync(Merchant merchant, string customerName)
    {
        merchant.PaymentAllowed = false;
        merchant.FinancialTransactionAllowed = false;

        await _repository.UpdateAsync(merchant);

        await _bus.Publish(new MerchantBlacklist
        {
            MerchantName = merchant.Name,
            CustomerName = customerName
        });
        
        await _auditLogService.AuditLogAsync(
                new AuditLog
                {
                    IsSuccess = true,
                    LogDate = DateTime.Now,
                    Operation = "MerchantBlacklistControlConsumer",
                    SourceApplication = "PF",
                    Resource = "Merchant",
                    UserId = Guid.Parse(merchant.CreatedBy),
                    Details = new Dictionary<string, string>
                    {
                                     {"MerchantId", merchant.Id.ToString() },
                                     {"PaymentAllowed", merchant.PaymentAllowed.ToString() },
                    }
                });
    }
}
