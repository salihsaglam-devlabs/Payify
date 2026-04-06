using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients
{
    public class DataEncryptionKeyHttpClient : HttpClientBase, IDataEncryptionKeyHttpClient
    {
        public DataEncryptionKeyHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor) :
            base(client, httpContextAccessor)
        {
        }

        public async Task DataEncryptionKeyAsync(DataEncryptionKeyRequest request)
        {
            var response = await PostAsJsonAsync($"v1/DataEncryptionKey", request);
            if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
        }
    }
}
