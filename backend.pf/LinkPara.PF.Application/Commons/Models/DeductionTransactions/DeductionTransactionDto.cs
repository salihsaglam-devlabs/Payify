using LinkPara.PF.Application.Commons.Mappings;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;

namespace LinkPara.PF.Application.Commons.Models.DeductionTransactions
{
    public class DeductionTransactionDto : IMapFrom<DeductionTransaction>
    {
        public Guid MerchantId { get; set; }
        public Guid PostingBalanceId { get; set; }
        public Guid MerchantDeductionId { get; set; }
        public DeductionType DeductionType { get; set; }
        public decimal Amount { get; set; }
        public int Currency { get; set; }
        public DateTime CreateDate { get; set; }
    }
}
