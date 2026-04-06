
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients
{
    public class MerchantDocumentHttpClient : HttpClientBase, IMerchantDocumentHttpClient
    {
        public MerchantDocumentHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor) : base(client, httpContextAccessor)
        {
        }

        public async Task<List<MerchantDocumentDto>> GetMerchantDocumentsByTransactionId(Guid transactionId)
        {
            var response = await GetAsync($"v1/MerchantDocument/transaction/{transactionId}");

            var merchantDocuments = await response.Content.ReadFromJsonAsync<List<MerchantDocumentDto>>();

            return merchantDocuments ?? throw new InvalidOperationException();
        }

        public async Task SaveMerchantDocumentsByTransactionId(SaveMerchantDocumentsByTransactionIdRequest request)
        {
            var response = await PostAsJsonAsync("v1/MerchantDocument", request);
            if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
        }
    }
}
