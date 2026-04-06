using LinkPara.HttpProviders.Emoney.Enums;

namespace LinkPara.HttpProviders.Emoney.Models
{
    public class TierLevelDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public TierLevelType TierLevelType { get; set; }
        public decimal MaxBalance { get; set; }
        public decimal DailyMaxInternalTransferAmount { get; set; }
        public int DailyMaxInternalTransferCount { get; set; }
        public decimal MonthlyMaxInternalTransferAmount { get; set; }
        public int MonthlyMaxInternalTransferCount { get; set; }
        public decimal DailyMaxDepositAmount { get; set; }
        public int DailyMaxDepositCount { get; set; }
        public decimal MonthlyMaxDepositAmount { get; set; }
        public int MonthlyMaxDepositCount { get; set; }
        public bool OwnIbanLimitEnabled { get; set; }
        public decimal DailyMaxWithdrawalAmount { get; set; }
        public int DailyMaxWithdrawalCount { get; set; }
        public decimal MonthlyMaxWithdrawalAmount { get; set; }
        public int MonthlyMaxWithdrawalCount { get; set; }
        public bool DistinctIbanWithdrawalLimitCheckEnabled { get; set; }
        public int DailyMaxDistinctIbanWithdrawalCount { get; set; }
        public int MonthlyMaxDistinctIbanWithdrawalCount { get; set; }
        public decimal DailyMaxInternationalTransferAmount { get; set; }
        public int DailyMaxInternationalTransferCount { get; set; }
        public decimal MonthlyMaxInternationalTransferAmount { get; set; }
        public int MonthlyMaxInternationalTransferCount { get; set; }
        public string CurrencySymbol { get; set; }
        public string RecordStatus { get; set; }
    }
}
