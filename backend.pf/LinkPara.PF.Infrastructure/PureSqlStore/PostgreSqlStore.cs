using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Models;
using LinkPara.PF.Domain.Enums;
using LinkPara.PF.Infrastructure.Persistence;
using LinkPara.SharedModels.Banking.Enums;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace LinkPara.PF.Infrastructure.PureSqlStore;

public class PostgreSqlStore : IPureSqlStore
{
    private readonly PfDbContext _dbContext;

    public PostgreSqlStore(PfDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid?> ReservePostingBalancesForMoneyTransferAsync(TimeSpan defaultTransferHour)
    {
        var now = DateTime.Now;
        var nowTotalMinutes = now.Hour * 60 + now.Minute;
        var processingId = Guid.NewGuid();

        var sql = """
                  WITH eligible AS (
                      SELECT pb.id, pb.merchant_id
                      FROM posting.balance pb
                      JOIN merchant.merchant m ON m.id = pb.merchant_id
                      WHERE
                          (
                              COALESCE(m.money_transfer_start_hour, @defaultHour) * 60 +
                              COALESCE(m.money_transfer_start_minute, @defaultMinute)
                          ) <= @nowTotalMinutes
                          AND m.payment_allowed = true
                          AND (
                              (
                                  pb.payment_date <= @currentDate
                                  AND (pb.batch_status = @batchStatusDeductionCalculated OR pb.batch_status = @batchStatusMoneyTransferPostponed)
                                  AND (pb.money_transfer_status = @moneyTransferPending OR pb.money_transfer_status = @moneyTransferBlocked)
                              )
                              OR
                              (
                                  pb.batch_status = @batchStatusMoneyTransferError
                                  AND pb.money_transfer_payment_date::date <> @currentDate
                                  AND pb.money_transfer_status = @moneyTransferPaymentError
                              )
                          )
                          AND pb.processing_id IS NULL
                  ),
                  merchant_groups AS (
                      SELECT merchant_id, gen_random_uuid() AS transaction_source_id
                      FROM eligible
                      GROUP BY merchant_id
                  )
                  UPDATE posting.balance pb
                  SET
                      batch_status = @batchStatusProcessing,
                      processing_id = @processingId,
                      transaction_source_id = mg.transaction_source_id,
                      processing_started_at = @now
                  FROM eligible e
                  JOIN merchant_groups mg ON e.merchant_id = mg.merchant_id
                  WHERE pb.id = e.id
                    AND pb.processing_id IS NULL
                  """;

        var affectedRows = await _dbContext.Database.ExecuteSqlRawAsync(
            sql,
            new NpgsqlParameter("defaultHour", defaultTransferHour.Hours),
            new NpgsqlParameter("defaultMinute", defaultTransferHour.Minutes),
            new NpgsqlParameter("nowTotalMinutes", nowTotalMinutes),
            new NpgsqlParameter("batchStatusDeductionCalculated", nameof(BatchStatus.DeductionCalculated)),
            new NpgsqlParameter("batchStatusMoneyTransferPostponed", nameof(BatchStatus.MoneyTransferPostponed)),
            new NpgsqlParameter("moneyTransferPending", nameof(PostingMoneyTransferStatus.Pending)),
            new NpgsqlParameter("moneyTransferBlocked", nameof(PostingMoneyTransferStatus.PaymentBlocked)),
            new NpgsqlParameter("batchStatusMoneyTransferError", nameof(BatchStatus.MoneyTransferError)),
            new NpgsqlParameter("moneyTransferPaymentError", nameof(PostingMoneyTransferStatus.PaymentError)),
            new NpgsqlParameter("batchStatusProcessing", nameof(BatchStatus.MoneyTransferProcessing)),
            new NpgsqlParameter("processingId", processingId),
            new NpgsqlParameter("now", now),
            new NpgsqlParameter("currentDate", now.Date)
        );

        return affectedRows > 0 ? processingId : null;
    }

    public async Task<Guid?> ReserveMerchantDeductionsAsync(List<Guid> merchantIds)
    {
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

        var sql = """
                  WITH eligible AS (
                      SELECT md.id
                      FROM merchant.merchant_deduction md
                      WHERE
                          md.merchant_id = ANY(@merchantIds)
                          AND md.deduction_status = @deductionStatusPending
                          AND md.execution_date <= @now
                          AND NOT (md.deduction_type = ANY(@invalidDeductionTypes))
                          AND md.processing_id IS NULL
                  )
                  UPDATE merchant.merchant_deduction md
                  SET
                      deduction_status = @deductionStatusProcessing,
                      processing_id = @processingId,
                      processing_started_at = @now
                  FROM eligible e
                  WHERE md.id = e.id
                    AND md.processing_id IS NULL
                  """;

        var affectedRows = await _dbContext.Database.ExecuteSqlRawAsync(
            sql,
            new NpgsqlParameter<Guid[]>("merchantIds", merchantIds.ToArray()),
            new NpgsqlParameter("deductionStatusPending", nameof(DeductionStatus.Pending)),
            new NpgsqlParameter<string[]>("invalidDeductionTypes", invalidDeductionTypes.ToArray()),
            new NpgsqlParameter("deductionStatusProcessing", nameof(DeductionStatus.Processing)),
            new NpgsqlParameter("processingId", processingId),
            new NpgsqlParameter("now", now)
        );

        return affectedRows > 0 ? processingId : null;
    }
}