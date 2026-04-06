using LinkPara.ApiGateway.BackOffice.Services.CampaignManagement.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.CampaignManagement.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.CampaignManagement.HttpClients
{
    public interface IChargeHttpClient
    {
        Task<PaginatedList<ChargeTransactionResponse>> GetChargeTransactionsAsync(GetChargeTransactionsSearchRequest request);
    }
}
