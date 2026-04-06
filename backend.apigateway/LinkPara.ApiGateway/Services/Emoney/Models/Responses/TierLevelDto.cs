using LinkPara.ApiGateway.Services.Emoney.Models.Enums;

namespace LinkPara.ApiGateway.Services.Emoney.Models.Responses;

public class TierLevelDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public TierLevelType TierLevelType { get; set; }  
    public bool MaxBalanceLimitEnabled { get; set; }
    public decimal MaxBalance { get; set; }
    public bool MaxInternalTransferLimitEnabled { get; set; }
    public decimal DailyMaxInternalTransferAmount { get; set; }
    public int DailyMaxInternalTransferCount { get; set; }
    public decimal MonthlyMaxInternalTransferAmount { get; set; }
    public int MonthlyMaxInternalTransferCount { get; set; }
    public bool MaxDepositLimitEnabled { get; set; }
    public decimal DailyMaxDepositAmount { get; set; }
    public int DailyMaxDepositCount { get; set; }
    public decimal MonthlyMaxDepositAmount { get; set; }
    public int MonthlyMaxDepositCount { get; set; }
    public bool MaxWithdrawalLimitEnabled { get; set; }
    public decimal DailyMaxWithdrawalAmount { get; set; }
    public int DailyMaxWithdrawalCount { get; set; }
    public decimal MonthlyMaxWithdrawalAmount { get; set; }
    public int MonthlyMaxWithdrawalCount { get; set; }
    public bool MaxOwnIbanWithdrawalLimitEnabled { get; set; }
    public int DailyMaxOwnIbanWithdrawalCount { get; set; }
    public int MonthlyMaxOwnIbanWithdrawalCount { get; set; }
    public bool MaxOtherIbanWithdrawalLimitEnabled { get; set; }
    public int DailyMaxOtherIbanWithdrawalCount { get; set; }
    public int DailyMaxDistinctOtherIbanWithdrawalCount { get; set; }
    public decimal DailyMaxOtherIbanWithdrawalAmount { get; set; }
    public int MonthlyMaxOtherIbanWithdrawalCount { get; set; }
    public int MonthlyMaxDistinctOtherIbanWithdrawalCount { get; set; }
    public decimal MonthlyMaxOtherIbanWithdrawalAmount { get; set; }
    public bool MaxInternationalTransferLimitEnabled { get; set; }
    public decimal DailyMaxInternationalTransferAmount { get; set; }
    public int DailyMaxInternationalTransferCount { get; set; }
    public decimal MonthlyMaxInternationalTransferAmount { get; set; }
    public int MonthlyMaxInternationalTransferCount { get; set; }
    public bool MaxOnUsPaymentLimitEnabled { get; set; }
    public decimal DailyMaxOnUsPaymentAmount { get; set; }
    public int DailyMaxOnUsPaymentCount { get; set; }
    public decimal MonthlyMaxOnUsPaymentAmount { get; set; }
    public int MonthlyMaxOnUsPaymentCount { get; set; }
    public string CurrencySymbol { get; set; }
    public string RecordStatus { get; set; }
}