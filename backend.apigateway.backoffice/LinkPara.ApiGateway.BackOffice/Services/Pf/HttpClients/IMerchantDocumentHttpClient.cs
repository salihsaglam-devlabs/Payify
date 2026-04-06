using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients
{
    public interface IMerchantDocumentHttpClient
    {
        Task<List<MerchantDocumentDto>> GetMerchantDocumentsByTransactionId(Guid transactionId);
        Task SaveMerchantDocumentsByTransactionId(SaveMerchantDocumentsByTransactionIdRequest request);
    }
}
