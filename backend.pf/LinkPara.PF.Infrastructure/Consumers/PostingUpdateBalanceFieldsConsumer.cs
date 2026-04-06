using LinkPara.PF.Application.Commons.Models.Posting;
using LinkPara.PF.Domain.Enums;
using LinkPara.PF.Infrastructure.Persistence;
using LinkPara.SharedModels.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LinkPara.PF.Infrastructure.Consumers;

public class PostingUpdateBalanceFieldsConsumer : IConsumer<PostingUpdateBalanceFields>
{
    private readonly ILogger<PostingUpdateBalanceFieldsConsumer> _logger;
    private readonly PfDbContext _dbContext;
    private readonly IBus _bus;
    
    public PostingUpdateBalanceFieldsConsumer(
        ILogger<PostingUpdateBalanceFieldsConsumer> logger,
        PfDbContext dbContext,
        IBus bus)
    {
        _logger = logger;
        _dbContext = dbContext;
        _bus = bus;
    }
    
    public async Task Consume(ConsumeContext<PostingUpdateBalanceFields> updateBalanceFields)
    {
        try
        {
            var bankBalances = await _dbContext
                .PostingBankBalance
                .Where(w => 
                    w.PostingDate == DateTime.Now.Date && 
                    w.BatchStatus == BatchStatus.Completed
                )
                .ToListAsync();
            
            foreach (var bankBalance in bankBalances)
            {
                var transactions = await _dbContext.PostingTransaction
                    .Where(s =>
                        (
                            (s.BatchStatus == BatchStatus.Pending && s.PostingDate == DateTime.Now.Date)
                            ||
                            s.BatchStatus == BatchStatus.Error
                        ) &&
                        s.RecordStatus == RecordStatus.Active &&
                        s.MerchantId == bankBalance.MerchantId &&
                        s.TransactionDate == bankBalance.TransactionDate &&
                        s.AcquireBankCode == bankBalance.AcquireBankCode &&
                        s.PaymentDate == bankBalance.PaymentDate &&
                        s.Currency == bankBalance.Currency &&
                        s.BlockageStatus == bankBalance.BlockageStatus
                    )
                    .OrderBy(x => x.Id)
                    .Select(x => x.Id)
                    .ToListAsync();
                
                var currentPage = 0;
                while (true)
                {
                    var pageResults = transactions
                        .Skip(currentPage * 1000)
                        .Take(1000)
                        .ToList();
            
                    if (pageResults.Count == 0)
                    {
                        break;
                    }
                    
                    try
                    {
                        using var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                        var busEndpoint = await _bus.GetSendEndpoint(new Uri("exchange:PF.PostingUpdateBalanceItems"));
                        await busEndpoint.Send(new PostingUpdateBalanceItems
                        {
                            PostingBankBalanceId = bankBalance.Id,
                            PostingBalanceId = bankBalance.PostingBalanceId,
                            PostingTransactionIds = pageResults
                        }, cancellationToken.Token);
                    }
                    catch (Exception exception)
                    {
                        _logger.LogCritical($"PostingUpdateBalanceFieldsError. BankBalanceId: {bankBalance.Id} Exception: {exception}");
                    }
                    
                    currentPage++;
                }
            }
        }
        catch (Exception exception)
        {
            _logger.LogCritical($"PostingUpdateBalanceFieldsError:{exception}");
        }
    }
}