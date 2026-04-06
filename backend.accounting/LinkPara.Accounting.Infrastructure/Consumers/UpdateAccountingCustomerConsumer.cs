using LinkPara.Accounting.Application.Commons.Interfaces;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Accounting;
using MassTransit;

namespace LinkPara.Accounting.Infrastructure.Consumers;

public class UpdateAccountingCustomerConsumer : IConsumer<UpdateAccountingCustomer>
{
    private readonly IAccountingService _accountingService;

    public UpdateAccountingCustomerConsumer(IAccountingService accountingService) 
        => _accountingService = accountingService;

    public async Task Consume(ConsumeContext<UpdateAccountingCustomer> context)
    {
        if (context.Message != null)
        {
            await _accountingService.UpdateAccountingCustomerAsync(context.Message);
        }
    }
}
