using LinkPara.SharedModels.Persistence;

namespace LinkPara.Emoney.Domain.Entities
{
    public class AccountFinancialInformation : AuditEntity  
    {
        public string IncomeSource { get; set; }
        public string IncomeInformation { get; set; }
        public string MonthlyTransactionVolume { get; set; }
        public string MonthlyTransactionCount { get; set; }
        public Guid AccountId { get; set; }
        public Account Account { get; set; }
    }
}
