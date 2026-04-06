using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients;

public class BulkOperationsHttpClient : HttpClientBase, IBulkOperationsHttpClient
{
    public BulkOperationsHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor)
        : base(client, httpContextAccessor)
    {
    }

    public async Task<BulkMerchantExcelValidationResponse> PreValidateBulkMerchantExcelFileAsync(BulkMerchantInsertServiceRequest request)
    {
        var response = await PostAsJsonAsync($"v1/BulkOperations/prevalidate-bulk-merchant", request);
        var result = await response.Content.ReadFromJsonAsync<BulkMerchantExcelValidationResponse>();
        return result ?? throw new InvalidOperationException();
    }

    public async Task<BulkImportMerchantResponse> BulkImportMerchantAsync(BulkMerchantInsertServiceRequest request)
    {
        var response = await PostAsJsonAsync($"v1/BulkOperations/import-bulk-merchant", request);
        var result = await response.Content.ReadFromJsonAsync<BulkImportMerchantResponse>();
        return result ?? throw new InvalidOperationException();
    }
}