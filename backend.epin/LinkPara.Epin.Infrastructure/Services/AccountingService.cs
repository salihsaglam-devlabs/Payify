using LinkPara.ContextProvider;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Accounting.Enums;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Accounting;
using MassTransit;
using Microsoft.Extensions.Logging;
using LinkPara.Epin.Application.Commons.Interfaces;
using LinkPara.Epin.Domain.Entities;

namespace LinkPara.Epin.Infrastructure.Services;

public class AccountingService : IAccountingService
{
    private readonly IBus _bus;
    private readonly IContextProvider _contextProvider;
    private readonly ILogger<AccountingService> _logger;

    public AccountingService(IBus bus,
        IContextProvider contextProvider,
        ILogger<AccountingService> logger)
    {
        _bus = bus;
        _contextProvider = contextProvider;
        _logger = logger;
    }

    public async Task PostAccountingPaymentAsync(Order order,string currencyCode, OperationType operationType)
    {
        try
        {
            var payment = new AccountingPayment
            {
                Amount = order.UnitPrice,
                CurrencyCode = currencyCode,
                Source = $"WA-{order.WalletNumber}",
                OperationType = operationType,
                UserId = Guid.Parse(_contextProvider.CurrentContext.UserId),
                TransactionDate = DateTime.Now,
                AccountingTransactionType = AccountingTransactionType.Emoney,
                AccountingCustomerType = AccountingCustomerType.Emoney
            };

            var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:Accounting.SavePayment"));
            await endpoint.Send(payment, tokenSource.Token);
        }
        catch (Exception exception)
        {
            _logger.LogError($"ErrorPostingAccountingTransaction: {exception}");
        }
    }
}
