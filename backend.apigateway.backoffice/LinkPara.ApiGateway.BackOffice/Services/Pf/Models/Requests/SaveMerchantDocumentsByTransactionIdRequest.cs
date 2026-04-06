using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests
{
    public class SaveMerchantDocumentsByTransactionIdRequest
    {
        public Guid TransactionId { get; set; }
        public Guid MerchantId { get; set; }
        public List<MerchantDocumentDto> MerchantDocuments { get; set; }
    }
}
