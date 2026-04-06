using System.Text.Json;
using LinkPara.PF.Pos.ApiGateway.Models.Requests;
using LinkPara.PF.Pos.ApiGateway.Models.Responses;
using LinkPara.SystemUser;

namespace LinkPara.PF.Pos.ApiGateway.Services.HttpClients;

public class PaxHttpClient : HttpClientBase, IPaxHttpClient
{
    public PaxHttpClient(HttpClient client, IApplicationUserService applicationUserService) 
        : base(client , applicationUserService)
    {
    }
    
    public async Task<TransactionResponse> PaxTransactionAsync(TransactionMerchantRequest request)
    {
        var response = await PostAsJsonAsync("v1/Pax/transaction", request);

        var responseString = await response.Content.ReadAsStringAsync();
        var provisionResponse = JsonSerializer.Deserialize<TransactionResponse>(responseString,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return provisionResponse;
    }

    public async Task<ParameterResponse> PaxParameterAsync(ParameterMerchantRequest request)
    {
        var response = await PostAsJsonAsync("v1/Pax/parameter", request);

        var responseString = await response.Content.ReadAsStringAsync();
        var parameterResponse = JsonSerializer.Deserialize<ParameterResponse>(responseString,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return parameterResponse;
    }

    public async Task<EndOfDayResponse> PaxEndOfDayAsync(EndOfDayMerchantRequest request)
    {
        var response = await PostAsJsonAsync("v1/Pax/eod", request);

        var responseString = await response.Content.ReadAsStringAsync();
        var endOfDayResponse = JsonSerializer.Deserialize<EndOfDayResponse>(responseString,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return endOfDayResponse;
    }

    public async Task<ReconciliationResponse> PaxReconciliationAsync(ReconciliationMerchantRequest request)
    {
        var response = await PostAsJsonAsync("v1/Pax/reconciliation", request);

        var responseString = await response.Content.ReadAsStringAsync();
        var reconciliationResponse = JsonSerializer.Deserialize<ReconciliationResponse>(responseString,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return reconciliationResponse;
    }
}