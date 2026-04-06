using LinkPara.Accounting.Application.Commons.Interfaces;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Accounting;
using MassTransit;

namespace LinkPara.Accounting.Infrastructure.Consumers;

public class CreateAccountingCustomerConsumer : IConsumer<AccountingCustomer>
{
    private readonly IAccountingService _accountingService;
    public CreateAccountingCustomerConsumer(IAccountingService accountingService)
    {
        _accountingService = accountingService;
    }
    public async Task Consume(ConsumeContext<AccountingCustomer> context)
    {
        await _accountingService.CreateCustomerAsync(context.Message);
    }
}
