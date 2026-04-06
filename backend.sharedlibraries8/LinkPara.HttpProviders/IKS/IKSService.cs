using LinkPara.HttpProviders.IKS.Models.Request;
using LinkPara.HttpProviders.IKS.Models.Response;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Json;

namespace LinkPara.HttpProviders.IKS
{
    public class IKSService : HttpClientBase, IIKSService
    {
        public IKSService(HttpClient client, IHttpContextAccessor httpContextAccessor) : base(client, httpContextAccessor)
        {
        }

        public async Task<IKSResponse<IKSAnnulmentResponse>> SaveAnnulmentAsync(IKSSaveAnnulmentRequest request)
        {
            var response = await PostAsJsonAsync("v1/Annulments", request);

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException();
            }

            var annulmentResponse = await response.Content.ReadFromJsonAsync<IKSResponse<IKSAnnulmentResponse>>();

            return annulmentResponse ?? throw new InvalidCastException();
        }

        public async Task<IKSResponse<IKSAnnulmentResponse>> UpdateAnnulmentAsync(IKSUpdateAnnulmentRequest request)
        {
            var response = await PutAsJsonAsync("v1/Annulments", request);

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException();
            }

            var annulmentResponse = await response.Content.ReadFromJsonAsync<IKSResponse<IKSAnnulmentResponse>>();

            return annulmentResponse ?? throw new InvalidCastException();
        }

        public async Task<IKSResponse<IKSMerchantResponse>> SaveMerchantAsync(IKSSaveMerchantRequest request)
        {
            var response = await PostAsJsonAsync("v1/Merchants", request);

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException();
            }

            var merchantResponse = await response.Content.ReadFromJsonAsync<IKSResponse<IKSMerchantResponse>>();

            return merchantResponse ?? throw new InvalidCastException();
        }

        public async Task<IKSResponse<IKSMerchantResponse>> UpdateMerchantAsync(IKSUpdateMerchantRequest request)
        {
            var response = await PutAsJsonAsync("v1/Merchants", request);

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException();
            }

            var merchantResponse = await response.Content.ReadFromJsonAsync<IKSResponse<IKSMerchantResponse>>();

            return merchantResponse ?? throw new InvalidCastException();
        }

        public async Task<IKSResponse<IKSTerminalResponse>> SaveTerminalAsync(IKSSaveTerminalRequest request)
        {
            var response = await PostAsJsonAsync("v1/Terminals", request);

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException();
            }

            var terminalResponse = await response.Content.ReadFromJsonAsync<IKSResponse<IKSTerminalResponse>>();

            return terminalResponse ?? throw new InvalidCastException();
        }
        public async Task<IKSResponse<IKSTerminalResponse>> UpdateTerminalAsync(IKSUpdateTerminalRequest request)
        {
            var response = await PutAsJsonAsync("v1/Terminals", request);

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException();
            }

            var terminalResponse = await response.Content.ReadFromJsonAsync<IKSResponse<IKSTerminalResponse>>();

            return terminalResponse ?? throw new InvalidCastException();
        }
        
        public async Task<IKSResponse<IKSTerminalResponse>> CreateTerminalAsync(IKSCreateTerminalRequest request)
        {
            var response = await PostAsJsonAsync("v1/Terminals/create-terminal", request);

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException();
            }

            var terminalResponse = await response.Content.ReadFromJsonAsync<IKSResponse<IKSTerminalResponse>>();

            return terminalResponse ?? throw new InvalidCastException();
        }
        
        public async Task<IKSResponse<IKSTerminalResponse>> GetTerminalStatusQueryAsync(IKSGetTerminalStatusRequest request)
        {
            var response = await GetAsync($"v1/Terminals/{request.ReferenceCode}");

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException();
            }

            var terminalResponse = await response.Content.ReadFromJsonAsync<IKSResponse<IKSTerminalResponse>>();

            return terminalResponse ?? throw new InvalidCastException();
        }

        public async Task<IKSResponse<AnnulmentsQueryResponse>> AnnulmentQueryAsync(IKSAnnulmentsQueryRequest request)
        {
            var response = await GetAsync($"v1/Annulments/annulmentsQuery?GlobalMerchantId={request.GlobalMerchantId}");

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException();
            }

            var annulmentResponse = await response.Content.ReadFromJsonAsync<IKSResponse<AnnulmentsQueryResponse>>();

            return annulmentResponse ?? throw new InvalidCastException();
        }
	}
}
