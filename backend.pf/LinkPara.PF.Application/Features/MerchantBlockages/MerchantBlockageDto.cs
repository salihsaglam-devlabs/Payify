using LinkPara.PF.Application.Commons.Mappings;
using LinkPara.PF.Application.Features.Merchants;
using LinkPara.PF.Application.Features.PostingBalances;
using LinkPara.PF.Application.Features.TimeoutTransactions;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;

namespace LinkPara.PF.Application.Features.MerchantBlockages
{
    public class MerchantBlockageDto : IMapFrom<MerchantBlockage>
    {
        public Guid Id { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal BlockageAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        public MerchantBlockageStatus MerchantBlockageStatus { get; set; }

        public Guid MerchantId { get; set; }
        public TransactionMerchantResponse Merchant { get; set; }
        public List<MerchantBlockageDetailDto> MerchantBlockageDetails { get; set; }
    }
}