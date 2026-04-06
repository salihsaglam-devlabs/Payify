using LinkPara.PF.Application.Commons.Mappings;
using LinkPara.PF.Application.Features.Merchants;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.PF.Application.Commons.Models.MerchantDeductions
{
    public class MerchantDeductionDto :IMapFrom<MerchantDeduction>
    {
        public Guid Id { get; set; }
        public Guid MerchantTransactionId { get; set; }
        public Guid MerchantId { get; set; }
        public MerchantDto Merchant { get; set; }
        public int Currency { get; set; }
        public decimal TotalDeductionAmount { get; set; }
        public decimal RemainingDeductionAmount { get; set; }
        public DateTime ExecutionDate { get; set; }
        public DeductionType DeductionType { get; set; }
        public DeductionStatus DeductionStatus { get; set; }
        public Guid MerchantDueId { get; set; }
        public RecordStatus RecordStatus { get; set; }
        public DateTime CreateDate { get; set; }
        public string ConversationId { get; set; }
        public decimal DeductionAmountWithCommission { get; set; }
        public Guid SubMerchantDeductionId { get; set; }
    }
}
