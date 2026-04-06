using LinkPara.ApiGateway.BackOffice.Commons.Extensions;
using LinkPara.ApiGateway.BackOffice.Commons.Helpers;
using LinkPara.ApiGateway.BackOffice.Services.Epin.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Epin.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Epin.HttpClients;

public class EpinReconciliationHttpClient : HttpClientBase, IEpinReconciliationHttpClient
{
    public EpinReconciliationHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor) : base(client, httpContextAccessor)
    {
    }

    public async Task<PaginatedList<EpinReconciliationSummaryDto>> GetFilterReconciliationSummariesAsync(GetFilterReconciliationSummariesRequest request)
    {
        var url = CreateUrlWithParams($"v1/Reconciliations/summary", request, true);
        var response = await GetAsync(url);
        var results = await response.Content.ReadFromJsonAsync<PaginatedList<EpinReconciliationSummaryDto>>();
        if (!CanSeeSensitiveData())
        {
            results.Items.ForEach(s =>
            {
                s.UnreconciledOrders?.ForEach(u =>
                {
                    u.UserFullName = SensitiveDataHelper.MaskSensitiveData("FullName", u.UserFullName);
                    u.Email = SensitiveDataHelper.MaskSensitiveData("Email", u.Email);
                });
            });
        }

        return results;
    }

    public async Task<EpinReconciliationSummaryDto> GetReconciliationSummaryAsync(Guid id)
    {
        var response = await GetAsync($"v1/Reconciliations/summary/{id}");
        var result = await response.Content.ReadFromJsonAsync<EpinReconciliationSummaryDto>();
        if (!CanSeeSensitiveData())
        {
            result.UnreconciledOrders?.ForEach(s =>
            {
                s.UserFullName = SensitiveDataHelper.MaskSensitiveData("FullName", s.UserFullName);
                s.Email = SensitiveDataHelper.MaskSensitiveData("Email", s.Email);
            });
        }

        return result;
    }

    public async Task ReconciliationByDateAsync(ReconciliationByDateRequest request)
    {
        await PostAsJsonAsync("v1/Reconciliations", request);
    }
}
