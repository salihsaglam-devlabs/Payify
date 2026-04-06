using System.Text.Json;
using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Models;
using LinkPara.PF.Domain.Enums;
using LinkPara.PF.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

namespace LinkPara.PF.Infrastructure.PureSqlStore;

public class MsSqlStore : IPureSqlStore
{
    private readonly PfDbContext _dbContext;

    public MsSqlStore(PfDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid?> ReservePostingBalancesForMoneyTransferAsync(TimeSpan defaultTransferHour)
    {
        var now = DateTime.Now;
        var nowTotalMinutes = now.Hour * 60 + now.Minute;
        var processingId = Guid.NewGuid();

        var sql = """
                  ;WITH eligible AS (
                      SELECT pb.Id, pb.MerchantId
                      FROM Posting.Balance pb WITH (UPDLOCK, READPAST, ROWLOCK)
                      JOIN Merchant.Merchant m ON m.Id = pb.MerchantId
                      WHERE
                          (
                              ISNULL(m.MoneyTransferStartHour, @defaultHour) * 60 +
                              ISNULL(m.MoneyTransferStartMinute, @defaultMinute)
                          ) <= @nowTotalMinutes
                          AND m.PaymentAllowed = 1
                          AND (
                              (
                                  pb.PaymentDate <= @currentDate
                                  AND (pb.BatchStatus = @batchStatusDeductionCalculated OR pb.BatchStatus = @batchStatusMoneyTransferPostponed)
                                  AND (pb.MoneyTransferStatus = @moneyTransferPending OR pb.MoneyTransferStatus = @moneyTransferBlocked)
                              )
                              OR
                              (
                                  pb.BatchStatus = @batchStatusMoneyTransferError
                                  AND CAST(pb.MoneyTransferPaymentDate AS DATE) <> @currentDate
                                  AND pb.MoneyTransferStatus = @moneyTransferPaymentError
                              )
                          )
                          AND pb.ProcessingId IS NULL
                  ),
                  merchant_groups AS (
                      SELECT DISTINCT MerchantId, NEWID() AS TransactionSourceId
                      FROM eligible
                  )
                  UPDATE pb
                  SET
                      pb.BatchStatus = @batchStatusProcessing,
                      pb.ProcessingId = @processingId,
                      pb.TransactionSourceId = mg.TransactionSourceId,
                      pb.ProcessingStartedAt = @now
                  OUTPUT @processingId AS ProcessingId
                  FROM Posting.Balance pb
                  JOIN eligible e ON pb.Id = e.Id
                  JOIN merchant_groups mg ON e.MerchantId = mg.MerchantId;
                  """;

        var result = await _dbContext.Database
            .SqlQueryRaw<ProcessingIdResult>(
                sql,
                new SqlParameter("defaultHour", defaultTransferHour.Hours),
                new SqlParameter("defaultMinute", defaultTransferHour.Minutes),
                new SqlParameter("nowTotalMinutes", nowTotalMinutes),
                new SqlParameter("batchStatusDeductionCalculated", nameof(BatchStatus.DeductionCalculated)),
                new SqlParameter("batchStatusMoneyTransferPostponed", nameof(BatchStatus.MoneyTransferPostponed)),
                new SqlParameter("moneyTransferPending", nameof(PostingMoneyTransferStatus.Pending)),
                new SqlParameter("moneyTransferBlocked", nameof(PostingMoneyTransferStatus.PaymentBlocked)),
                new SqlParameter("batchStatusMoneyTransferError", nameof(BatchStatus.MoneyTransferError)),
                new SqlParameter("moneyTransferPaymentError", nameof(PostingMoneyTransferStatus.PaymentError)),
                new SqlParameter("batchStatusProcessing", nameof(BatchStatus.MoneyTransferProcessing)),
                new SqlParameter("processingId", processingId),
                new SqlParameter("now", now),
                new SqlParameter("currentDate", now.Date)
            )
            .FirstOrDefaultAsync();

        return result?.ProcessingId;
    }

    public async Task<Guid?> ReserveMerchantDeductionsAsync(List<Guid> merchantIds)
    {
        if (merchantIds == null || merchantIds.Count == 0)
        {
            return null;
        }

        var invalidDeductionTypes = new List<string>
        {
            nameof(DeductionType.RejectedChargeback),
            nameof(DeductionType.RejectedChargebackCommission),
            nameof(DeductionType.RejectedChargebackTransfer),
            nameof(DeductionType.RejectedSuspicious),
            nameof(DeductionType.RejectedSuspiciousCommission),
            nameof(DeductionType.RejectedSuspiciousTransfer)
        };

        var now = DateTime.Now;
        var processingId = Guid.NewGuid();
        var merchantIdsJson = JsonSerializer.Serialize(merchantIds);
        var invalidDeductionTypesJson = JsonSerializer.Serialize(invalidDeductionTypes);

        var sql = """
                  ;WITH eligible AS (
                      SELECT md.Id
                      FROM Merchant.MerchantDeduction md WITH (UPDLOCK, READPAST, ROWLOCK)
                      WHERE
                          md.MerchantId IN (
                              SELECT CAST(value AS UNIQUEIDENTIFIER)
                              FROM OPENJSON(@merchantIds)
                          )
                          AND md.DeductionStatus = @deductionStatusPending
                          AND md.ExecutionDate <= @now
                          AND md.DeductionType NOT IN (
                              SELECT value FROM OPENJSON(@invalidDeductionTypes)
                          )
                          AND md.ProcessingId IS NULL
                  )
                  UPDATE md
                  SET
                      md.DeductionStatus = @deductionStatusProcessing,
                      md.ProcessingId = @processingId,
                      md.ProcessingStartedAt = @now
                  OUTPUT @processingId AS ProcessingId
                  FROM Merchant.MerchantDeduction md
                  JOIN eligible e ON md.Id = e.Id;
                  """;

        var result = await _dbContext.Database.SqlQueryRaw<ProcessingIdResult>(
            sql,
            new SqlParameter("merchantIds", merchantIdsJson),
            new SqlParameter("deductionStatusPending", nameof(DeductionStatus.Pending)),
            new SqlParameter("invalidDeductionTypes", invalidDeductionTypesJson),
            new SqlParameter("deductionStatusProcessing", nameof(DeductionStatus.Processing)),
            new SqlParameter("processingId", processingId),
            new SqlParameter("now", now)
        ).FirstOrDefaultAsync();

        return result?.ProcessingId;
    }
}