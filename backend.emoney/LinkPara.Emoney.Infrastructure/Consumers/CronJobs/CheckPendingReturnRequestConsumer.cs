using LinkPara.Emoney.Application.Commons.Strategies;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.Emoney.Domain.Enums;
using LinkPara.Emoney.Infrastructure.Persistence;
using LinkPara.HttpProviders.DbProvider;
using LinkPara.HttpProviders.MoneyTransfer.Models;
using LinkPara.SharedModels.Banking.Enums;
using LinkPara.SharedModels.BusModels.Commands.Scheduler;
using LinkPara.SystemUser;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
namespace LinkPara.Emoney.Infrastructure.Consumers.CronJobs;

public class CheckPendingReturnRequestConsumer : IConsumer<CheckPendingReturnRequest>
{
    private readonly ILogger<CheckPendingReturnRequestConsumer> _logger;
    private readonly IApplicationUserService _applicationUserService;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IBus _bus;
    private readonly IDatabaseProviderService _databaseProviderService;

    public CheckPendingReturnRequestConsumer(ILogger<CheckPendingReturnRequestConsumer> logger,
        IApplicationUserService applicationUserService,
        IServiceScopeFactory scopeFactory,
        IBus bus,
        IDatabaseProviderService databaseProviderService)
    {
        _logger = logger;
        _applicationUserService = applicationUserService;
        _scopeFactory = scopeFactory;
        _bus = bus;
        _databaseProviderService = databaseProviderService;
    }

    public async Task Consume(ConsumeContext<CheckPendingReturnRequest> context)
    {
        var pendingRequests = await GetPendingReturnRequestsAsync();

        foreach (var pendingRequest in pendingRequests)
        {
            await PublishTransferRequestAsync(pendingRequest);
        }
    }

    private async Task<List<ReturnTransactionRequest>> GetPendingReturnRequestsAsync()
    {
        using var scope = _scopeFactory.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<EmoneyDbContext>();

        var sql = string.Empty;

        var databaseProvider = await _databaseProviderService.GetProviderAsync();

        if (databaseProvider == "MsSql")
        {
            var strategy = new NoRetryExecutionStrategy(dbContext);

            var result = await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await dbContext.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted);

                try
                {
                    sql = @"WITH CTE AS (
                                    SELECT TOP (1000) *
                                    FROM Core.ReturnTransactionRequest WITH (ROWLOCK, READPAST, UPDLOCK)
                                    WHERE 
                                        Status IN ('Pending', 'NotDelivered')
                                        AND RecordStatus = 'Active'
                                        AND IsProcessed = 0
                                )
                                UPDATE CTE
                                SET IsProcessed = 1
                                OUTPUT inserted.*;";

                    var requests = await dbContext.ReturnTransactionRequest
                                        .FromSqlRaw(sql)
                                        .ToListAsync();

                    await transaction.CommitAsync();

                    return requests;
                }
                catch (Exception exception)
                {
                    await transaction.RollbackAsync();

                    _logger.LogError("GetPendingReturnRequestsAsync Exception : {Exception}", exception);

                    throw;
                }
            });

            return result;
        }
        else
        {
            sql = @"UPDATE core.return_transaction_request 
                    SET is_processed=true
                    WHERE 
	                    id IN (SELECT id FROM core.return_transaction_request wr 
				                    WHERE 
                                    status IN('Pending','NotDelivered')
				                    AND record_status = 'Active' 
				                    AND is_processed=false FOR UPDATE SKIP LOCKED)
                    RETURNING *;";

            var requests = await dbContext.ReturnTransactionRequest
                                .FromSqlRaw(sql)
                                .ToListAsync();


            return requests;
        }
    }

    private async Task PublishTransferRequestAsync(ReturnTransactionRequest request)
    {
        try
        {
            var transferRequest = new SaveTransferRequest
            {
                Amount = request.Amount,
                CurrencyCode = request.CurrencyCode,
                Description = request.Description,
                ReceiverIBAN = request.ReceiverIbanNumber,
                ReceiverName = request.ReceiverName,
                Source = TransactionSource.Emoney,
                TransactionSourceReferenceId = request.Id,
                UserId = Guid.Parse(request.CreatedBy),
                IncomingTransactionId = request.IncomingTransactionId,
                IsReturnPayment = true,
            };

            var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(10));

            var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:MoneyTransfer.SaveTransfer"));

            await endpoint.Send(transferRequest, cancellationToken.Token);
        }
        catch (Exception exception)
        {
            _logger.LogError($"SaveTransferRequest Error : {exception}");

            await MarkAsNotDeliveredAsync(request);
        }
    }

    private async Task MarkAsNotDeliveredAsync(ReturnTransactionRequest returnRequest)
    {
        using var scope = _scopeFactory.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<EmoneyDbContext>();

        returnRequest.UpdateDate = DateTime.Now;
        returnRequest.LastModifiedBy = _applicationUserService.ApplicationUserId.ToString();
        returnRequest.Status = ReturnTransactionStatus.NotDelivered;
        returnRequest.IsProcessed = false;

        dbContext.Update(returnRequest);

        await dbContext.SaveChangesAsync();
    }
}
