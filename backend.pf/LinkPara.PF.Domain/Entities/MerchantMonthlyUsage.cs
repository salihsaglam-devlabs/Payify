using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.PF.Domain.Entities
{

    public class MerchantMonthlyUsage : AuditEntity
    {
        public DateTime Date { get; set; }
        public int Count { get; set; }
        public decimal Amount { get; set; }
        public Guid MerchantId { get; set; }
        public Merchant Merchant { get; set; }
        public string Currency { get; set; }
        public TransactionLimitType TransactionLimitType { get; set; }
    }
}
