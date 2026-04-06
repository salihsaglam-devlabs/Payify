using LinkPara.Billing.Application.Commons.Interfaces;
using LinkPara.Billing.Domain.Entities;
using LinkPara.ContextProvider;
using LinkPara.HttpProviders.Emoney;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Accounting.Enums;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Accounting;
using MassTransit;
using Microsoft.Extensions.Logging;
using LinkPara.HttpProviders.Accounting;

namespace LinkPara.Billing.Infrastructure.Services.AccountingServices;

public class AccountingService : IAccountingService
{
    private readonly IBus _bus;
    private readonly IContextProvider _contextProvider;
    private readonly ILogger<AccountingService> _logger;
    private readonly IPaymentService _paymentService;

    private readonly int _bankCode = 59;

    public AccountingService(IBus bus,
        IContextProvider contextProvider,
        ILogger<AccountingService> logger,
        IPaymentService paymentService)
    {
        _bus = bus;
        _contextProvider = contextProvider;
        _logger = logger;
        _paymentService = paymentService;
    }

    public async Task PostAccountingPaymentAsync(Transaction transaction, Guid emoneyTransactionId)
    {
        try
        {
            var payment = new AccountingPayment
            {
                Amount = transaction.BillAmount,
                CommissionAmount = transaction.CommissionAmount,
                BankCode = _bankCode,
                HasCommission = transaction.CommissionAmount > 0,
                CurrencyCode = transaction.Currency,
                Source = $"WA-{transaction.WalletNumber}",
                OperationType = OperationType.BillPayment,
                UserId = Guid.Parse(_contextProvider.CurrentContext.UserId),
                TransactionDate = DateTime.Now,
                AccountingCustomerType = AccountingCustomerType.Emoney,
                ClientReferenceId = transaction.AccountingReferenceId,
                AccountingTransactionType = AccountingTransactionType.Billing,
                BsmvAmount = transaction.BsmvAmount,
                TransactionId = emoneyTransactionId
            };

            using var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:Accounting.SavePayment"));
            await endpoint.Send(payment, tokenSource.Token);

        }
        catch (Exception exception)
        {
            _logger.LogError("ErrorPostingAccountingTransaction: {exception}", exception);
        }
    }

    public async Task CancelAccountingPaymentAsync(Transaction transaction)
    {
        try
        {
            if (transaction.CommissionAmount > 0)
            {
                await _paymentService.CancelPaymentAsync(transaction.AccountingReferenceId);
            }
            else
            {
                var payment = new AccountingPayment
                {
                    Amount = transaction.BillAmount,
                    CommissionAmount = transaction.CommissionAmount,
                    BankCode = _bankCode,
                    HasCommission = transaction.CommissionAmount > 0,
                    CurrencyCode = transaction.Currency,
                    Destination = $"WA-{transaction.WalletNumber}",
                    OperationType = OperationType.BillPaymentCancellation,
                    UserId = Guid.Parse(_contextProvider.CurrentContext.UserId),
                    TransactionDate = DateTime.Now,
                    AccountingCustomerType = AccountingCustomerType.Emoney,
                    ClientReferenceId = transaction.AccountingReferenceId,
                    AccountingTransactionType = AccountingTransactionType.Billing,
                    TransactionId = transaction.Id
                };

                using var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:Accounting.SavePayment"));
                await endpoint.Send(payment, tokenSource.Token);

            }
        }
        catch (Exception exception)
        {
            _logger.LogError("ErrorPostingAccountingCancelTransaction: {exception}", exception);
        }
    }
}