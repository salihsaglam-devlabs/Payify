using DocumentFormat.OpenXml.Office2010.Excel;
using LinkPara.ApiGateway.BackOffice.Commons.Helpers;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.JsonPatch;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients;

public class MerchantReturnPoolHttpClient : HttpClientBase, IMerchantReturnPoolHttpClient
{
    public MerchantReturnPoolHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor) 
        : base(client, httpContextAccessor)
    {
    }

    public async Task<PaginatedList<MerchantReturnPoolDto>> GetFilterListAsync(GetMerchantReturnPoolsRequest request)
    {
        var url = CreateUrlWithParams($"v1/MerchantReturnPool", request, true);
        var response = await GetAsync(url);
        var returnPoolResponses = await response.Content.ReadFromJsonAsync<PaginatedList<MerchantReturnPoolDto>>();

        if (!CanSeeSensitiveData())
        {
            returnPoolResponses.Items.ForEach(s =>
            {
                s.CardNumber = SensitiveDataHelper.MaskSensitiveData("CardNumber", s.CardNumber);
            });
        }

        return returnPoolResponses ?? throw new InvalidOperationException();
    }

    public async Task<ReturnResponse> ActionMerchantReturnPoolAsync(ActionMerchantReturnPoolRequest request)
    {
        var response = await PostAsJsonAsync($"v1/MerchantReturnPool", request);
        var returnResponse = await response.Content.ReadFromJsonAsync<ReturnResponse>();
        return returnResponse ?? throw new InvalidOperationException();
    }
}