using LinkPara.Billing.Application.Commons.Exceptions;
using LinkPara.Billing.Application.Commons.Interfaces;
using LinkPara.Billing.Application.Commons.Models.Billing;
using LinkPara.Billing.Domain.Entities;
using LinkPara.Billing.Domain.Enums;
using LinkPara.SharedModels.BusModels.Commands.Scheduler;
using LinkPara.SharedModels.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LinkPara.Billing.Infrastructure.Consumers.CronJobs;

public class TimeoutConsumer : IConsumer<BillingTimeout>
{
    private readonly int _maxTryCount = 5;

    private readonly IGenericRepository<TimeoutTransaction> _timeoutRepository;
    private readonly IBillingService _billingService;
    private readonly ILogger<TimeoutConsumer> _logger;

    public TimeoutConsumer(IGenericRepository<TimeoutTransaction> timeoutRepository,
        IBillingService billingService,
        ILogger<TimeoutConsumer> logger)
    {
        _timeoutRepository = timeoutRepository;
        _billingService = billingService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<BillingTimeout> context) => await ProcessTimeoutsAsync();

    private async Task ProcessTimeoutsAsync()
    {
        var timeoutItems = await GetTimeoutItemsAsync();

        foreach (var timeoutItem in timeoutItems)
        {
            await ProcessTimeoutItemAsync(timeoutItem);
        }
    }

    private async Task ProcessTimeoutItemAsync(TimeoutTransaction timeoutItem)
    {
        try
        {
            var billStatusResponse = await _billingService.InquireBillStatusAsync(timeoutItem.TransactionId);

            if (billStatusResponse.IsSuccess)
            {
                var billStatus = billStatusResponse.Response.BillStatus;

                timeoutItem.TimeoutTransactionType = billStatus switch
                {
                    BillStatus.Paid or
                    BillStatus.AwaitingPaymentConfirmation => TimeoutTransactionType.CancelPayment,
                    BillStatus.PaymentCancelled or
                    BillStatus.AwaitingCancelConfirmation => TimeoutTransactionType.NoAction,
                    _ => throw new ArgumentException($"UnexpectedBillStatus: {billStatus}, TransactionId: {timeoutItem.TransactionId}")
                };

                timeoutItem.UpdateDate = DateTime.Now;

                await ExecuteTimeoutActionAsync(timeoutItem);
                await _timeoutRepository.UpdateAsync(timeoutItem);
            }
            else
            {
                await OnErrorAsync(timeoutItem, $"ErrorCode: {billStatusResponse.ErrorCode}, ErrorMessage: {billStatusResponse.ErrorMessage}");
            }
        }
        catch (NotPaidException notPaidException)
        {
            timeoutItem.TimeoutTransactionType = TimeoutTransactionType.NoAction;
            timeoutItem.UpdateDate = DateTime.Now;
            timeoutItem.Description = notPaidException.Message;
            timeoutItem.TimeoutTransactionStatus = TimeoutTransactionStatus.Finished;

            await _timeoutRepository.UpdateAsync(timeoutItem);
        }
        catch (Exception exception)
        {
            await OnErrorAsync(timeoutItem, $"ErrorProcessingBillingTimeoutItem: Id: {timeoutItem.Id}, Error: {exception.Message}");
        }
    }

    private async Task ExecuteTimeoutActionAsync(TimeoutTransaction timeoutItem)
    {
        if (timeoutItem.TimeoutTransactionType == TimeoutTransactionType.NoAction)
        {
            timeoutItem.TimeoutTransactionStatus = TimeoutTransactionStatus.Finished;
        }
        else if (timeoutItem.TimeoutTransactionType == TimeoutTransactionType.CancelPayment)
        {
            var billCancelResponse = await _billingService.CancelBillPaymentAsync(timeoutItem.TransactionId, $"CancelledFromTimeout");

            if (billCancelResponse.IsSuccess)
            {
                timeoutItem.TimeoutTransactionStatus = TimeoutTransactionStatus.Finished;
                timeoutItem.Description = billCancelResponse.Response.BillCancelInvoice.Description;
            }
            else
            {
                await OnErrorAsync(timeoutItem, $"ErrorExecutingTimeoutCancelAction: TransactionId: {timeoutItem.TransactionId}");
            }
        }
    }

    private async Task OnErrorAsync(TimeoutTransaction timeoutTransaction, string errorMessage)
    {
        try
        {
            if (timeoutTransaction.RetryCount < _maxTryCount)
            {
                timeoutTransaction.RetryCount++;
                timeoutTransaction.NextTryTime = DateTime.Now.AddMinutes(timeoutTransaction.RetryCount * 1);
            }
            else
            {
                timeoutTransaction.TimeoutTransactionStatus = TimeoutTransactionStatus.Failed;

                _logger.LogCritical("TimeoutRetryExceededMaxCount, TimeoutId: {Id}, Error: {errorMessage}", timeoutTransaction.Id, errorMessage);
            }

            timeoutTransaction.UpdateDate = DateTime.Now;

            await _timeoutRepository.UpdateAsync(timeoutTransaction);
        }
        catch (Exception exception)
        {

            _logger.LogCritical("ErrorExecutingOnErrorForTimeout: {TransactionId}, Error: {ErrorMessage}, Exception: {Exception}",
      timeoutTransaction.Id, errorMessage, exception);

        }
    }

    private async Task<List<TimeoutTransaction>> GetTimeoutItemsAsync()
    {
        try
        {
            return await _timeoutRepository
            .GetAll()
            .Where(t => t.RetryCount < _maxTryCount
                && t.NextTryTime <= DateTime.Now
                && t.TimeoutTransactionStatus == TimeoutTransactionStatus.Pending)
            .ToListAsync();
        }
        catch (Exception exception)
        {
            _logger.LogCritical("ErrorGettingTimeoutItems: {exception}", exception);

            return new List<TimeoutTransaction>();
        }
    }
}