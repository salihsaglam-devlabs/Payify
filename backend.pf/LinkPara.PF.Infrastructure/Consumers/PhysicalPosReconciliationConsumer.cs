using System.Transactions;
using LinkPara.PF.Application.Commons.Exceptions;
using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Models.PhysicalPos;
using LinkPara.PF.Application.Commons.Models.PhysicalPos.Constants;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Entities.PhysicalPos;
using LinkPara.PF.Domain.Enums;
using LinkPara.PF.Domain.Enums.PhysicalPos;
using LinkPara.PF.Infrastructure.Persistence;
using LinkPara.SharedModels.Notification.NotificationModels.PF;
using LinkPara.SharedModels.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TransactionStatus = LinkPara.PF.Domain.Enums.TransactionStatus;

namespace LinkPara.PF.Infrastructure.Consumers;

public class PhysicalPosReconciliationConsumer : IConsumer<PhysicalPosReconciliation>
{
    private readonly ILogger<PhysicalPosReconciliationConsumer> _logger;
    private readonly PfDbContext _dbContext;
    private readonly IBus _bus;
    private readonly IResponseCodeService _errorCodeService;

    public PhysicalPosReconciliationConsumer(
        ILogger<PhysicalPosReconciliationConsumer> logger,
        PfDbContext dbContext,
        IBus bus, IResponseCodeService errorCodeService)
    {
        _logger = logger;
        _dbContext = dbContext;
        _bus = bus;
        _errorCodeService = errorCodeService;
    }

    public async Task Consume(ConsumeContext<PhysicalPosReconciliation> context)
    {
        var batchEod = await _dbContext.PhysicalPosEndOfDay
            .FirstOrDefaultAsync(s => s.Id == context.Message.PhysicalPosEodId);
        if (batchEod is null)
        {
            _logger.LogError("Reconciliation Consumer: BatchEod not found for Id: {EodId}",
                context.Message.PhysicalPosEodId);
            return;
        }
        if (batchEod.Status != EndOfDayStatus.Reconciliation)
        {
            _logger.LogError("EndOfDay batch is not in reconciliation state EodId: {EodId}", batchEod.Id);
            return;
        }
        
        try
        {
            var reconciliationTransactions = await _dbContext.PhysicalPosReconciliationTransaction
                .Where(s => context.Message.ReconciliationTransactionIds.Contains(s.Id) &&
                            s.RecordStatus == RecordStatus.Active)
                .ToListAsync();

            var paymentIds = reconciliationTransactions.Select(s => s.PaymentId).ToList();
            var bankRefs = reconciliationTransactions.Select(s => s.BankRef).ToList();
            var rrnNumbers = reconciliationTransactions.Select(s => s.Rrn).ToList();
            var posMerchantIds = reconciliationTransactions.Select(s => s.MerchantId).ToList();

            var existingBankTransactions = await _dbContext.BankTransaction.Where(s =>
                    s.TransactionStatus == TransactionStatus.Success &&
                    posMerchantIds.Contains(s.SubMerchantCode) &&
                    paymentIds.Contains(s.OrderId) &&
                    bankRefs.Contains(s.BankOrderId) &&
                    rrnNumbers.Contains(s.RrnNumber) &&
                    s.PhysicalPosEodId == batchEod.Id)
                .ToListAsync();
            var existingBankTransactionIds = existingBankTransactions.Select(s => s.Id).ToList();

            //list of transactions that we have in the system but no record on reconciliation request
            var excessBankTransactions = await _dbContext.BankTransaction.Where(s =>
                    s.TransactionStatus == TransactionStatus.Success &&
                    !existingBankTransactionIds.Contains(s.Id) &&
                    s.PhysicalPosEodId == batchEod.Id)
                .ToListAsync();
            
            var excessMerchantTransactionIds = excessBankTransactions.Select(s => s.MerchantTransactionId).ToList();
            var excessMerchantTransactions = await _dbContext.MerchantTransaction
                .Where(s => excessMerchantTransactionIds.Contains(s.Id))
                .ToListAsync();

            var existingMerchantTransactionIds =
                existingBankTransactions.Select(s => s.MerchantTransactionId).Distinct().ToList();
            var existingMerchantTransactions = await _dbContext.MerchantTransaction
                .Where(s => existingMerchantTransactionIds.Contains(s.Id))
                .ToListAsync();

            var unacceptableTransactions = await _dbContext.PhysicalPosUnacceptableTransaction
                .Where(s =>
                    s.BatchId == batchEod.BatchId &&
                    paymentIds.Contains(s.PaymentId) &&
                    posMerchantIds.Contains(s.MerchantId) &&
                    bankRefs.Contains(s.BankRef) &&
                    rrnNumbers.Contains(s.Rrn))
                .ToListAsync();

            var newUnacceptableTransactions = new List<PhysicalPosUnacceptableTransaction>();

            var strategy = _dbContext.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

                if (excessBankTransactions.Count > 0)
                {
                    HandleExcessBankTransactions(excessBankTransactions, excessMerchantTransactions);
                }

                var authTransactions = reconciliationTransactions
                    .Where(s => s.Type is PaxTransactionType.Sale or PaxTransactionType.InstallmentSale)
                    .ToList();

                foreach (var recon in authTransactions)
                {
                    await ProcessReconciliationTransaction(recon, existingBankTransactions, existingMerchantTransactions,
                        unacceptableTransactions, batchEod, newUnacceptableTransactions);
                }

                var refundTransactions = reconciliationTransactions
                    .Where(s => s.Type == PaxTransactionType.Refund)
                    .ToList();

                foreach (var recon in refundTransactions)
                {
                    await ProcessReconciliationTransaction(recon, existingBankTransactions, existingMerchantTransactions,
                        unacceptableTransactions, batchEod, newUnacceptableTransactions);
                }

                var voidTransactions = reconciliationTransactions
                    .Where(s => s.Type == PaxTransactionType.Void)
                    .ToList();

                foreach (var recon in voidTransactions)
                {
                    await ProcessReconciliationTransaction(recon, existingBankTransactions, existingMerchantTransactions,
                        unacceptableTransactions, batchEod, newUnacceptableTransactions);
                }

                var pendingReconciliationTransactions = reconciliationTransactions
                    .Where(s => s.ReconciliationStatus == ReconciliationStatus.Pending).ToList();
                pendingReconciliationTransactions.ForEach(s =>
                { s.ReconciliationStatus = ReconciliationStatus.ActionRequired; });

                var pendingBankTransactions = existingBankTransactions
                    .Where(s => s.EndOfDayStatus is EndOfDayStatus.Pending or EndOfDayStatus.Reconciliation).ToList();
                pendingBankTransactions.ForEach(s =>
                    { s.EndOfDayStatus = EndOfDayStatus.ActionRequired; });
                
                var pendingMerchantTransactions = existingMerchantTransactions
                    .Where(s => s.EndOfDayStatus is EndOfDayStatus.Pending or EndOfDayStatus.Reconciliation).ToList();
                pendingMerchantTransactions.ForEach(s =>
                    { s.EndOfDayStatus = EndOfDayStatus.ActionRequired; });
                
                var pendingUnacceptableTransactions = unacceptableTransactions
                    .Where(s => s.EndOfDayStatus is EndOfDayStatus.Pending or EndOfDayStatus.Reconciliation).ToList();
                pendingUnacceptableTransactions.ForEach(s =>
                    { s.EndOfDayStatus = EndOfDayStatus.ActionRequired; });

                var hasActionRequired =
                    reconciliationTransactions.Any(s => s.ReconciliationStatus == ReconciliationStatus.ActionRequired);

                if (hasActionRequired || 
                    excessBankTransactions.Count > 0 || 
                    unacceptableTransactions.Count > 0 || 
                    newUnacceptableTransactions.Count > 0)
                {
                    batchEod.Status = EndOfDayStatus.ActionRequired;
                }
                else
                {
                    batchEod.Status = EndOfDayStatus.Completed;
                }
                
                _dbContext.PhysicalPosReconciliationTransaction.UpdateRange(reconciliationTransactions);
                _dbContext.BankTransaction.UpdateRange(existingBankTransactions);
                _dbContext.BankTransaction.UpdateRange(excessBankTransactions);
                _dbContext.MerchantTransaction.UpdateRange(existingMerchantTransactions);
                _dbContext.PhysicalPosUnacceptableTransaction.UpdateRange(unacceptableTransactions);
                _dbContext.PhysicalPosUnacceptableTransaction.AddRange(newUnacceptableTransactions);
                _dbContext.PhysicalPosEndOfDay.Update(batchEod);
                
                await _dbContext.SaveChangesAsync();
                scope.Complete();
            });
        }
        catch (Exception exception)
        {
            _logger.LogError($"EndOfDay Reconciliation Consumer Error: {exception}");
            await PublishReconciliationFailedNotificationAsync(batchEod);
        }
    }

    private async Task ProcessReconciliationTransaction(
        PhysicalPosReconciliationTransaction recon,
        List<BankTransaction> existingBankTransactions,
        List<MerchantTransaction> existingMerchantTransactions,
        List<PhysicalPosUnacceptableTransaction> unacceptableTransactions,
        PhysicalPosEndOfDay batchEod,
        List<PhysicalPosUnacceptableTransaction> newUnacceptableTransactions)
    {
        var expectedTransactionType = recon.Type switch
        {
            PaxTransactionType.Sale or PaxTransactionType.InstallmentSale => TransactionType.Auth,
            PaxTransactionType.Refund => TransactionType.Return,
            PaxTransactionType.Void => TransactionType.Reverse,
            _ => TransactionType.Auth
        };

        var matchedBankTransaction = existingBankTransactions.FirstOrDefault(s =>
            s.SubMerchantCode == recon.MerchantId &&
            s.OrderId == recon.PaymentId &&
            s.BankOrderId == recon.BankRef &&
            s.RrnNumber == recon.Rrn &&
            s.TransactionType == expectedTransactionType &&
            s.PhysicalPosEodId == batchEod.Id);

        if (matchedBankTransaction is not null)
        {
            var matchedMerchantTransaction =
                existingMerchantTransactions.First(s => s.Id == matchedBankTransaction.MerchantTransactionId);

            if (matchedBankTransaction.Amount == recon.Amount)
            {
                matchedMerchantTransaction.EndOfDayStatus = EndOfDayStatus.Completed;
                if(matchedMerchantTransaction.BatchStatus == BatchStatus.EodPending)
                    matchedMerchantTransaction.BatchStatus = BatchStatus.Pending;
                matchedBankTransaction.EndOfDayStatus = EndOfDayStatus.Completed;
                recon.ReconciliationStatus = ReconciliationStatus.Reconciled;
                recon.MerchantTransactionId = matchedMerchantTransaction.Id;
            }
            else
            {
                matchedMerchantTransaction.EndOfDayStatus = EndOfDayStatus.ActionRequired;
                matchedBankTransaction.EndOfDayStatus = EndOfDayStatus.ActionRequired;
                recon.ReconciliationStatus = ReconciliationStatus.ActionRequired;
                var apiResponse =
                    await _errorCodeService.GetApiResponseCode(ApiErrorCode.ReconciliationAmountMismatch, "TR");
                recon.ErrorCode = ApiErrorCode.ReconciliationAmountMismatch;
                recon.ErrorMessage = apiResponse.DisplayMessage;
                recon.MerchantTransactionId = matchedMerchantTransaction.Id;
                _logger.LogError(
                    "Amount mismatch for ReconciliationTransaction {ReconId}. Expected: {Expected}, Found: {Found}",
                    recon.Id, recon.Amount, matchedBankTransaction.Amount);
            }

            return;
        }

        var matchedUnacceptable = unacceptableTransactions.FirstOrDefault(s =>
            s.PaymentId == recon.PaymentId &&
            s.MerchantId == recon.MerchantId &&
            s.BankRef == recon.BankRef &&
            s.Rrn == recon.Rrn &&
            s.Type == recon.Type);

        if (matchedUnacceptable is not null)
        {
            if (matchedUnacceptable.Amount == recon.Amount)
            {
                matchedUnacceptable.EndOfDayStatus = EndOfDayStatus.Completed;
                recon.ReconciliationStatus = ReconciliationStatus.Reconciled;
                recon.UnacceptableTransactionId = matchedUnacceptable.Id;
            }
            else
            {
                matchedUnacceptable.EndOfDayStatus = EndOfDayStatus.ActionRequired;
                recon.ReconciliationStatus = ReconciliationStatus.ActionRequired;
                var apiResponse =
                    await _errorCodeService.GetApiResponseCode(ApiErrorCode.ReconciliationAmountMismatch, "TR");
                recon.ErrorCode = ApiErrorCode.ReconciliationAmountMismatch;
                recon.ErrorMessage = apiResponse.DisplayMessage;
                recon.UnacceptableTransactionId = matchedUnacceptable.Id;
                _logger.LogError(
                    "Amount mismatch for ReconciliationTransaction {ReconId}. Expected: {Expected}, Found: {Found}",
                    recon.Id, recon.Amount, matchedUnacceptable.Amount);
            }

            if (matchedUnacceptable.Type == PaxTransactionType.Void)
            {
                var suspendedBankTransaction =
                    existingBankTransactions.FirstOrDefault(s => s.BankOrderId == matchedUnacceptable.OriginalRef);
                if (suspendedBankTransaction is not null)
                {
                    var suspendedMerchantTransaction =
                        existingMerchantTransactions.First(s => s.Id == suspendedBankTransaction.MerchantTransactionId);
                    if(suspendedMerchantTransaction.BatchStatus == BatchStatus.Pending)
                        suspendedMerchantTransaction.BatchStatus = BatchStatus.EodPending;
                    suspendedBankTransaction.EndOfDayStatus = EndOfDayStatus.Suspended;
                    suspendedMerchantTransaction.EndOfDayStatus = EndOfDayStatus.Suspended;
                }
            }

            return;
        }

        var newUnacceptableResponse =
            await _errorCodeService.GetApiResponseCode(ApiErrorCode.ReconciliationNewTransaction, "TR");
        recon.ErrorCode = ApiErrorCode.ReconciliationNewTransaction;
        recon.ErrorMessage = newUnacceptableResponse.DisplayMessage;
        recon.ReconciliationStatus = ReconciliationStatus.ActionRequired;
        var newUnacceptableTransaction = PopulateNewUnacceptableTransaction(recon);
        newUnacceptableTransactions.Add(newUnacceptableTransaction);
        recon.UnacceptableTransactionId = newUnacceptableTransaction.Id;
        if (newUnacceptableTransaction.Type == PaxTransactionType.Void)
        {
            var suspendedBankTransaction =
                existingBankTransactions.FirstOrDefault(s => s.BankOrderId == newUnacceptableTransaction.OriginalRef);
            if (suspendedBankTransaction is not null)
            {
                var suspendedMerchantTransaction =
                    existingMerchantTransactions.First(s => s.Id == suspendedBankTransaction.MerchantTransactionId);
                suspendedBankTransaction.EndOfDayStatus = EndOfDayStatus.Suspended;
                suspendedMerchantTransaction.EndOfDayStatus = EndOfDayStatus.Suspended;
            }
        }
    }

    private static PhysicalPosUnacceptableTransaction PopulateNewUnacceptableTransaction(
        PhysicalPosReconciliationTransaction recon)
    {
        return new PhysicalPosUnacceptableTransaction
        {
            PaymentId = recon.PaymentId,
            BatchId = recon.BatchId,
            Date = recon.Date,
            Type = recon.Type,
            Status = recon.Status,
            Currency = recon.Currency,
            MerchantId = recon.MerchantId,
            TerminalId = recon.TerminalId,
            Amount = recon.Amount,
            PointAmount = recon.PointAmount,
            Installment = recon.Installment,
            MaskedCardNo = recon.MaskedCardNo,
            BinNumber = recon.BinNumber,
            ProvisionNo = recon.ProvisionNo,
            AcquirerResponseCode = recon.AcquirerResponseCode,
            InstitutionId = recon.InstitutionId,
            Vendor = recon.Vendor,
            Rrn = recon.Rrn,
            Stan = recon.Stan,
            PosEntryMode = recon.PosEntryMode,
            PinEntryInfo = recon.PinEntryInfo,
            BankRef = recon.BankRef,
            OriginalRef = recon.OriginalRef,
            PfMerchantId = recon.PfMerchantId,
            ConversationId = recon.ConversationId,
            ClientIpAddress = recon.ClientIpAddress,
            SerialNumber = recon.SerialNumber,
            Gateway = string.Empty,
            ErrorCode = recon.ErrorCode,
            ErrorMessage = recon.ErrorMessage,
            CurrentStatus = UnacceptableTransactionStatus.ActionRequired,
            PhysicalPosEodId = recon.PhysicalPosEodId,
            EndOfDayStatus = EndOfDayStatus.Completed,
            MerchantTransactionId = Guid.Empty,
            CreatedBy = "RECONCILIATION_BATCH"
        };
    }

    private void HandleExcessBankTransactions(List<BankTransaction> bankTransactions, List<MerchantTransaction> merchantTransactions)
    {
        bankTransactions.ForEach(s => { s.EndOfDayStatus = EndOfDayStatus.ActionRequired;});
        merchantTransactions.ForEach(s => { s.EndOfDayStatus = EndOfDayStatus.ActionRequired;});
    }

    private async Task PublishReconciliationFailedNotificationAsync(
        PhysicalPosEndOfDay batchEod)
    {
        var merchant = await _dbContext.Merchant.FirstOrDefaultAsync(s => s.Id == batchEod.MerchantId);
    
        await _bus.Publish(new PhysicalPosReconciliationFailed
        {
            Date = batchEod.Date,
            Vendor = batchEod.Vendor,
            SerialNumber = batchEod.SerialNumber,
            MerchantId = batchEod.MerchantId,
            MerchantName = merchant?.Name,
            MerchantNumber = merchant?.Number,
            PosMerchantId = batchEod.PosMerchantId,
            PosTerminalId = batchEod.PosTerminalId,
            BatchId = batchEod.BatchId
        }, CancellationToken.None);
    }
}