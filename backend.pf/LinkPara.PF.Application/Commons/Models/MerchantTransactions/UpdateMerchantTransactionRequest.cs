using LinkPara.PF.Application.Commons.Mappings;
using LinkPara.PF.Application.Commons.Models.Merchants;
using LinkPara.PF.Domain.Entities;

namespace LinkPara.PF.Application.Commons.Models.MerchantTransactions
{
    public class UpdateMerchantTransactionRequest : IMapFrom<MerchantTransaction>
    {
        public bool IsChargeback { get; set; }
        public bool IsSuspecious { get; set; }
        public string SuspeciousDescription { get; set; }
        public List<MerchantDocumentDto> Files { get; set; }
    }
}
