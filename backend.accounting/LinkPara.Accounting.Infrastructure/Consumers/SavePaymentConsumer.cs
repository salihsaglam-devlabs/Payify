using LinkPara.Accounting.Application.Commons.Interfaces;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Accounting;
using MassTransit;

namespace LinkPara.Accounting.Infrastructure.Consumers;

public class SavePaymentConsumer : IConsumer<AccountingPayment>
{
    private readonly IAccountingService _accountingService;
    public SavePaymentConsumer(IAccountingService accountingService)
    {
        _accountingService = accountingService;
    }
    public async Task Consume(ConsumeContext<AccountingPayment> context)
    {
        await _accountingService.PostPaymentAsync(context.Message);
    }
}
