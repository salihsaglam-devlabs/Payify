using LinkPara.PF.Application.Commons.Models.Posting;
using LinkPara.PF.Infrastructure.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LinkPara.PF.Infrastructure.Consumers;

public class UpdateTransactionPaymentDateConsumer : IConsumer<UpdateTransactionPaymentDate>
{
    private readonly PfDbContext _dbContext;
    private readonly ILogger<UpdateTransactionPaymentDateConsumer> _logger;

    public UpdateTransactionPaymentDateConsumer(
        PfDbContext dbContext, 
        ILogger<UpdateTransactionPaymentDateConsumer> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<UpdateTransactionPaymentDate> context)
    {
        try
        {
            await _dbContext
                .PostingTransaction
                .Where(a => context.Message.PostingTransactionIds.Contains(a.Id))
                .ExecuteUpdateAsync(u => u
                    .SetProperty(p => p.PaymentDate, context.Message.PaymentDate)
                );
            
            await _dbContext
                .MerchantTransaction
                .Where(a => context.Message.MerchantTransactionIds.Contains(a.Id))
                .ExecuteUpdateAsync(u => u
                    .SetProperty(p => p.PfPaymentDate, context.Message.PaymentDate)
                );
        }
        catch (Exception exception)
        {
            _logger.LogCritical($"UpdateTransactionPaymentDateException. Exception: {exception}");
        }
    }
}

