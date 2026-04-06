using LinkPara.ApiGateway.BackOffice.Commons.Helpers;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients;

public class TimeoutTransactionHttpClient : HttpClientBase, ITimeoutTransactionHttpClient
{
    public TimeoutTransactionHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor)
        : base(client, httpContextAccessor)
    {
    }

    public async Task<PaginatedList<TimeoutTransactionDto>> GetAllAsync(GetFilterTransactionTimeoutRequest request)
    {
        var url = CreateUrlWithParams($"v1/TimeoutTransactions", request, true);
        var response = await GetAsync(url);
        var timeoutTransactions = await response.Content.ReadFromJsonAsync<PaginatedList<TimeoutTransactionDto>>();

        if (!CanSeeSensitiveData())
        {
            timeoutTransactions.Items.ForEach(s =>
            {
                s.CardNumber = SensitiveDataHelper.MaskSensitiveData("CardNumber", s.CardNumber);
            });
        }

        return timeoutTransactions ?? throw new InvalidOperationException();
    }
}
