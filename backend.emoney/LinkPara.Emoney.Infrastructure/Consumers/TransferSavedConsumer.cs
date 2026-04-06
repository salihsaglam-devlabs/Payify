using LinkPara.Emoney.Domain.Enums;
using LinkPara.Emoney.Infrastructure.Persistence;
using LinkPara.HttpProviders.MoneyTransfer.Models;
using LinkPara.SystemUser;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LinkPara.Emoney.Infrastructure.Consumers;

public class TransferSavedConsumer : IConsumer<SaveTransferResponse>
{
    private readonly ILogger<TransferSavedConsumer> _logger;
    private readonly IApplicationUserService _applicationUserService;
    private readonly IServiceScopeFactory _scopeFactory;

    public TransferSavedConsumer(ILogger<TransferSavedConsumer> logger,
        IApplicationUserService applicationUserService,
        IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _applicationUserService = applicationUserService;
        _scopeFactory = scopeFactory;
    }

    public async Task Consume(ConsumeContext<SaveTransferResponse> context)
    {
        using var scope = _scopeFactory.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<EmoneyDbContext>();

        var response = context.Message;

        if (response.IsReturnPayment)
        {
            await UpdateReturnTransactionStatusAsync(dbContext, response);            
        }
        else
        {
            await UpdateWithdrawRequestStatusAsync(dbContext, response);
        }        
    }

    private async Task UpdateWithdrawRequestStatusAsync(EmoneyDbContext dbContext, SaveTransferResponse response)
    {
        var request = await dbContext.WithdrawRequest
                        .FirstOrDefaultAsync(s => s.Id == response.TransactionSourceId);

        if (request == null)
        {
            _logger.LogError(
                $"TransferSavedConsumer Error : Withdraw Request Not Found! Id = {response.TransactionSourceId}");
        }
        else
        {
            request.UpdateDate = DateTime.Now;
            request.LastModifiedBy = _applicationUserService.ApplicationUserId.ToString();
            request.WithdrawStatus = WithdrawStatus.Delivered;

            dbContext.Update(request);
            await dbContext.SaveChangesAsync();
        }
    }

    private async Task UpdateReturnTransactionStatusAsync(EmoneyDbContext dbContext, SaveTransferResponse response)
    {
        var request = await dbContext.ReturnTransactionRequest
                        .FirstOrDefaultAsync(s => s.Id == response.TransactionSourceId);

        if (request == null)
        {
            _logger.LogError(
                $"TransferSavedConsumer Error : Return Transaction Not Found! Id = {response.TransactionSourceId}");
        }
        else
        {
            request.UpdateDate = DateTime.Now;
            request.LastModifiedBy = _applicationUserService.ApplicationUserId.ToString();
            request.Status = ReturnTransactionStatus.Delivered;

            dbContext.Update(request);
            await dbContext.SaveChangesAsync();
        }
    }
}
