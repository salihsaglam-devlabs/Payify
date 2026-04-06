using System.ComponentModel.DataAnnotations.Schema;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.Emoney.Domain.Entities;

public class AccountCurrentLevel : AuditEntity
{
    public Guid AccountId { get; set; }
    public Guid TierLevelId { get; set; }
    public DateTime LevelDate { get; set; }
    public decimal DailyInternalTransferAmount { get; set; }
    public int DailyInternalTransferCount { get; set; }
    public decimal MonthlyInternalTransferAmount { get; set; }
    public int MonthlyInternalTransferCount { get; set; }
    public decimal DailyDepositAmount { get; set; }
    public int DailyDepositCount { get; set; }
    public decimal MonthlyDepositAmount { get; set; }
    public int MonthlyDepositCount { get; set; }
    public decimal DailyWithdrawalAmount { get; set; }
    public int DailyWithdrawalCount { get; set; }
    public decimal MonthlyWithdrawalAmount { get; set; }
    public int MonthlyWithdrawalCount { get; set; }
    public int DailyOwnIbanWithdrawalCount { get; set; }
    public int MonthlyOwnIbanWithdrawalCount { get; set; }
    public int DailyOtherIbanWithdrawalCount { get; set; }
    public int DailyDistinctOtherIbanWithdrawalCount { get; set; }
    public decimal DailyOtherIbanWithdrawalAmount { get; set; }
    public int MonthlyOtherIbanWithdrawalCount { get; set; }
    public int MonthlyDistinctOtherIbanWithdrawalCount { get; set; }
    public decimal MonthlyOtherIbanWithdrawalAmount { get; set; }
    public decimal DailyInternationalTransferAmount { get; set; }
    public int DailyInternationalTransferCount { get; set; }
    public decimal MonthlyInternationalTransferAmount { get; set; }
    public int MonthlyInternationalTransferCount { get; set; }
    public string CurrencyCode { get; set; }
    public Currency Currency { get; set; }
    public decimal DailyOnUsPaymentAmount { get; set; }
    public int DailyOnUsPaymentCount { get; set; }
    public decimal MonthlyOnUsPaymentAmount { get; set; }
    public int MonthlyOnUsPaymentCount { get; set; }
}