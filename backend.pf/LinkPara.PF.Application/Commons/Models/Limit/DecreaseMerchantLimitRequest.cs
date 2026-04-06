
using LinkPara.PF.Domain.Enums;

namespace LinkPara.PF.Application.Commons.Models.Limit
{
    public class DecreaseMerchantLimitRequest
    {
        public Guid MerchantId { get; set; }
        public Guid? SubMerchantId { get; set; }
        public decimal Amount { get; set; }
        public DateTime TransactionDate { get; set; }
        public TransactionType TransactionType { get; set; }
    }
}
