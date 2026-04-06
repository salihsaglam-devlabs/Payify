using LinkPara.HttpProviders.Accounting;
using LinkPara.PF.Domain.Entities;
using LinkPara.SharedModels.BusModels.Commands.Scheduler;
using LinkPara.SharedModels.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LinkPara.PF.Infrastructure.Consumers.CronJobs;

public class PostingBillUrlConsumer : IConsumer<PostingBillUrlJob>
{
    private readonly IInvoiceService _invoiceService;
    private readonly IGenericRepository<PostingBill> _postingBillRepository;
    private readonly ILogger<PostingBillUrlConsumer> _logger;

    public PostingBillUrlConsumer(IInvoiceService invoiceService, 
        IGenericRepository<PostingBill> postingBillRepository, 
        ILogger<PostingBillUrlConsumer> logger)
    {
        _invoiceService = invoiceService;
        _postingBillRepository = postingBillRepository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PostingBillUrlJob> context)
    {
        try
        {
            var bills = await _postingBillRepository
                .GetAll()
                .Where(w => string.IsNullOrEmpty(w.BillUrl))
                .ToListAsync();

            await Parallel.ForEachAsync(bills, new ParallelOptions { MaxDegreeOfParallelism = 10 }, async (bill, token) =>
            {
                try
                {
                    var invoice = await _invoiceService.GetInvoiceAsync(bill.ClientReferenceId);
                    
                    if (invoice.IsSuccess)
                    {
                        bill.BillUrl = invoice.BillUrl;
                    }
                }
                catch (Exception exception)
                {
                    _logger.LogError($"ErrorCallingGetInvoiceServiceForClientReference: {bill.ClientReferenceId}" +
                        $"Error: {exception}");
                }
            });

            await _postingBillRepository.UpdateRangeAsync(bills);
        }
        catch (Exception exception)
        {
            _logger.LogError($"BillUrlConsumerError: {exception}");
        }
    }
}