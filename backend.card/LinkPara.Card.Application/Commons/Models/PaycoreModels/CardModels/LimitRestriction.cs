namespace LinkPara.Card.Application.Commons.Models.PaycoreModels.CardModels
{
    public class LimitRestriction
    {
        public string StmtUsageType { get; set; }
        public int StmtMaxAmount { get; set; }
        public int StmtMaxCount { get; set; }
        public int StmtMaxRatio { get; set; }
        public string TxnEffectType { get; set; }
        public int CurrencyCode { get; set; }
        public string EffectType { get; set; }
        public string TxnRegion { get; set; }
        public string TerminalType { get; set; }
        public string Description { get; set; }
        public string AtOnceUsageType { get; set; }
        public int MaxAmountAtOnce { get; set; }
        public int MaxRatioAtOnce { get; set; }
        public string DailyUsageType { get; set; }
        public int DailyMaxAmount { get; set; }
        public int DailyMaxCount { get; set; }
        public int DailyMaxRatio { get; set; }
        public string WeeklyUsageType { get; set; }
        public int WeeklyMaxAmount { get; set; }
        public int WeeklyMaxCount { get; set; }
        public int WeeklyMaxRatio { get; set; }
        public string MonthlyUsageType { get; set; }
        public int MonthlyMaxAmount { get; set; }
        public int MonthlyMaxCount { get; set; }
        public int MonthlyMaxRatio { get; set; }
        public string YearlyUsageType { get; set; }
        public int YearlyMaxAmount { get; set; }
        public int YearlyMaxCount { get; set; }
        public int YearlyMaxRatio { get; set; }
    }
}
