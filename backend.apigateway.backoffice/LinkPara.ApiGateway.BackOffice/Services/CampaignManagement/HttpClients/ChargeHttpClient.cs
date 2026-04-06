using LinkPara.ApiGateway.BackOffice.Commons.Helpers;
using LinkPara.ApiGateway.BackOffice.Services.CampaignManagement.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.CampaignManagement.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.CampaignManagement.HttpClients;

public class ChargeHttpClient : HttpClientBase, IChargeHttpClient
{
    public ChargeHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor) : base(client, httpContextAccessor)
    {
    }

    public async Task<PaginatedList<ChargeTransactionResponse>> GetChargeTransactionsAsync(GetChargeTransactionsSearchRequest request)
    {
        var url = CreateUrlWithParams($"v1/IWalletCharge", request, true);
        var response = await GetAsync(url);

        var chargeTransactions = await response.Content.ReadFromJsonAsync<PaginatedList<ChargeTransactionResponse>>();

        if (!CanSeeSensitiveData())
        {
            chargeTransactions.Items.ForEach(s =>
            {
                s.FullName = SensitiveDataHelper.MaskSensitiveData("FullName", s.FullName != null ? s.FullName : string.Empty);
                s.CardNumber = SensitiveDataHelper.MaskSensitiveData("CardNumber", s.CardNumber != null ? s.CardNumber : string.Empty);
            });
        }

        return chargeTransactions;
    }
}
