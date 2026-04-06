using LinkPara.ApiGateway.Services.CampaignManagement.Models.Responses;

namespace LinkPara.ApiGateway.Services.CampaignManagement.HttpClients
{
    public interface IIWalletAgreementHttpClient
    {
        Task<List<IWalletAgreementResponse>> GetAgreementsAsync();
    }
}
