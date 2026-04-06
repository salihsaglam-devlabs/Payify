using LinkPara.ApiGateway.Commons.MultiFactorModels;
using LinkPara.HttpProviders.MultiFactor.Models.PowerFactorModels.Request;
using LinkPara.HttpProviders.MultiFactor.Models.PowerFactorModels.Response;

namespace LinkPara.ApiGateway.Services.MultiFactor;

public class MultiFactorTransactionHttpClient : HttpClientBase, IMultiFactorTransactionHttpClient
{
    public MultiFactorTransactionHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor)
        : base(client, httpContextAccessor)
    {
    }

    public async Task<StartClientTransactionResponse> StartClientTransaction(StartClientTransactionRequest request)
    {
        var response = await PostAsJsonAsync("v1/transaction/start-client-transaction", request);
            
        return await response.Content.ReadFromJsonAsync<StartClientTransactionResponse>();
    }

    public async Task<CheckTransactionApprovalResponse> CheckTransactionApproval(CheckTransactionApprovalRequest request)
    {
        var response = await PostAsJsonAsync("v1/transaction/check-transaction-approval", request);
            
        return await response.Content.ReadFromJsonAsync<CheckTransactionApprovalResponse>();
    }

    public async Task<OneTouchTransactionResponse> StartOneTouchTransaction(StartOneTouchTransactionRequest request)
    {
        var response = await PostAsJsonAsync("v1/transaction/start-one-touch-transaction", request);
            
        return await response.Content.ReadFromJsonAsync<OneTouchTransactionResponse>();
    }
}