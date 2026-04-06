using LinkPara.ApiGateway.Card.Services.Card.Models;

namespace LinkPara.ApiGateway.Card.Services.Card.HttpClients;
public class DebitAuthorizationHttpClient : HttpClientBase, IDebitAuthorizationHttpClient
{
    public DebitAuthorizationHttpClient(HttpClient client, IHttpContextAccessor contextAccessor) 
        : base(client, contextAccessor)
    {
    }

    public async Task<DebitAuthorizationResponse> ProcessDebitAuthorizationAsync(DebitAuthorizationRequest request)
    {
        var response = await PostAsJsonAsync($"v1/PaycoreDebitAuthorization/debit-auth", request);

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException();

        var debitAuthorizationResponse = await response.Content.ReadFromJsonAsync<DebitAuthorizationResponse>();

        return debitAuthorizationResponse ?? throw new InvalidOperationException();
    }
}
