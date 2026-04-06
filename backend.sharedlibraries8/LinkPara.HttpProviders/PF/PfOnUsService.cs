using System.Net.Http.Json;
using LinkPara.HttpProviders.PF.Models.Request;
using LinkPara.HttpProviders.PF.Models.Response;
using LinkPara.HttpProviders.Utility;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;

namespace LinkPara.HttpProviders.PF;

public class PfOnUsService : HttpClientBase, IPfOnUsService
{
    public PfOnUsService(HttpClient client, IHttpContextAccessor httpContextAccessor)
        : base(client, httpContextAccessor)
    {
    }
    
    public async Task<VerifyOnUsPaymentResponse> VerifyOnUsPaymentAsync(VerifyOnUsPaymentRequest request)
    {
        var response = await PostAsJsonAsync("v1/Payments/verify-onus-payment", request);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException();
        }

        var provisionResponse = await response.Content.ReadFromJsonAsync<VerifyOnUsPaymentResponse>();

        return provisionResponse ?? throw new InvalidOperationException();
    }
    
    public async Task<UpdateMerchantTransactionRequest> ChargebackOnUsPayment(Guid merchantTransactionId, JsonPatchDocument<UpdateMerchantTransactionRequest> merchantTransactionRequest)
    {
        var response = await PatchAsync($"v1/MerchantTransactions/{merchantTransactionId}", merchantTransactionRequest);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException();
        }

        var merchantTransaction = await response.Content.ReadFromJsonAsync<UpdateMerchantTransactionRequest>();

        return merchantTransaction ?? throw new InvalidOperationException();
    }

    public async Task<PaginatedList<MerchantDeductionDto>> GetOnUsPaymentDeductions(GetOnUsPaymentDeductionsRequest merchantDeductionsRequest)
    {
        var url = GetQueryString.CreateUrlWithSearchQueryParams($"v1/MerchantDeduction", merchantDeductionsRequest, true);

        var response = await GetAsync(url);

        var deductions = await response.Content.ReadFromJsonAsync<PaginatedList<MerchantDeductionDto>>();

        return deductions ?? throw new InvalidOperationException();
    }
}