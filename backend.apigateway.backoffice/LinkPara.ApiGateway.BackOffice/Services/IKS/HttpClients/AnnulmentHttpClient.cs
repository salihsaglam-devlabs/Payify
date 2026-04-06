using LinkPara.ApiGateway.BackOffice.Services.IKS.Models.Request;
using LinkPara.ApiGateway.BackOffice.Services.IKS.Models.Response;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;

namespace LinkPara.ApiGateway.BackOffice.Services.IKS.HttpClients
{
    public class AnnulmentHttpClient : HttpClientBase, IAnnulmentHttpClient
    {
        public AnnulmentHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor)
        : base(client, httpContextAccessor)
        {

        }

        public async Task<IKSResponse<AnnulmentCodesResponse>> AnnulmentCodesAsync()
        {
            var response = await GetAsync($"v1/Annulments");
            var annulmentCodes = await response.Content.ReadFromJsonAsync<IKSResponse<AnnulmentCodesResponse>>();
            return annulmentCodes ?? throw new InvalidOperationException();
        }

        public async Task<IKSResponse<AnnulmentsQueryResponse>> AnnulmentsQueryAsync(AnnulmentsQueryRequest request)
        {
            var response = await GetAsync($"v1/Annulments/annulmentsQuery?GlobalMerchantId={request.GlobalMerchantId}");
            var annulmentsQuery = await response.Content.ReadFromJsonAsync<IKSResponse<AnnulmentsQueryResponse>>();
            return annulmentsQuery ?? throw new InvalidOperationException();
        }

        public async Task<IKSResponse<CardBinResponse>> GetCardBinAsync()
        {
            var response = await GetAsync($"v1/Annulments/cardBins");
            var cardBins = await response.Content.ReadFromJsonAsync<IKSResponse<CardBinResponse>>();
            return cardBins ?? throw new InvalidOperationException();
        }
    }
}
