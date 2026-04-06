using LinkPara.PF.Application.Commons.Models.Posting;
using LinkPara.PF.Domain.Entities;
using LinkPara.SharedModels.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LinkPara.PF.Infrastructure.Consumers;

public class PublishTransactionPaymentDateConsumer : IConsumer<PublishTransactionPaymentDate>
{
    private readonly IGenericRepository<PostingTransaction> _postingTransactionRepository;
    private readonly ILogger<PublishTransactionPaymentDateConsumer> _logger;
    private readonly IBus _bus;
    private const int TransactionPerBatch = 500;

    public PublishTransactionPaymentDateConsumer(
        IGenericRepository<PostingTransaction> postingTransactionRepository, 
        ILogger<PublishTransactionPaymentDateConsumer> logger, 
        IBus bus)
    {
        _postingTransactionRepository = postingTransactionRepository;
        _logger = logger;
        _bus = bus;
    }

    public async Task Consume(ConsumeContext<PublishTransactionPaymentDate> context)
    {
        try
        {
            var currentPage = 0;

            var transactionsQuery = _postingTransactionRepository
                .GetAll()
                .Where(s => context.Message.PostingBalanceIds.Contains(s.PostingBalanceId))
                .OrderBy(x => x.Id)
                .Select(s => new { PostingTransactionId = s.Id, MerchantTransactionId = s.MerchantTransactionId });
        
            while (true)
            {
                var pageResults = await transactionsQuery
                    .Skip(currentPage * TransactionPerBatch)
                    .Take(TransactionPerBatch)
                    .AsNoTracking()
                    .ToListAsync();
            
                if (pageResults.Count == 0)
                {
                    break;
                }
            
                try
                {
                    using var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                    var busEndpoint = await _bus.GetSendEndpoint(new Uri("exchange:PF.UpdateTransactionPaymentDate"));
                    await busEndpoint.Send(new UpdateTransactionPaymentDate
                    {
                        PostingTransactionIds = pageResults.Select(s => s.PostingTransactionId).ToList(),
                        MerchantTransactionIds = pageResults.Select(s => s.MerchantTransactionId).ToList(),
                        PaymentDate = context.Message.PaymentDate,
                    }, cancellationToken.Token);
                }
                catch (Exception exception)
                {
                    _logger.LogCritical($"UpdateTransactionPaymentDatePublishException. Exception: {exception}");
                }

                currentPage++;
            }
        }
        catch (Exception exception)
        {
            _logger.LogCritical($"UpdateTransactionPaymentDateException. Exception: {exception}");
        }
    }
}