using LinkPara.CampaignManagement.Application.Commons.Interfaces;
using LinkPara.ContextProvider;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Accounting.Enums;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Accounting;
using MassTransit;
using MassTransit.Transports;
using Microsoft.Extensions.Logging;

namespace LinkPara.CampaignManagement.Infrastructure.Services;

public class AccountingService : IAccountingService
{
    private readonly IBus _bus;
    private readonly ILogger<AccountingService> _logger;

    public AccountingService(IBus bus,
        ILogger<AccountingService> logger)
    {
        _bus = bus;
        _logger = logger;
    }

    public async Task PostAccountingPaymentAsync(AccountingPayment payment)
    {
        try
        {
            var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:Accounting.SavePayment"));
            await endpoint.Send(payment, tokenSource.Token);
        }
        catch (Exception exception)
        {
            _logger.LogError("ErrorPostingAccountingTransaction: {Exception}", exception);
        }
    }
}
