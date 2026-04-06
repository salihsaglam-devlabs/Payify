using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests
{
    public class UpdateMerchantTransactionRequest
    {
        public bool IsChargeback { get; set; }
        public bool IsSuspecious { get; set; }
        public string SuspeciousDescription { get; set; }
        public List<MerchantDocumentDto> Files { get; set; }
    }
}
