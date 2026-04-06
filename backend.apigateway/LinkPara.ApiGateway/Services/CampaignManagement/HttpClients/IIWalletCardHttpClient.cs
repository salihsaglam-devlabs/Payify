using LinkPara.ApiGateway.Services.CampaignManagement.Models.Requests;
using LinkPara.ApiGateway.Services.CampaignManagement.Models.Responses;

namespace LinkPara.ApiGateway.Services.CampaignManagement.HttpClients;

public interface IIWalletCardHttpClient
{
    Task<IWalletQrCodeResponse> CreateCardAsync(IWalletCreateCardRequest request);
    Task<IWalletCardResponse> GetUserIWalletCardAsync(GetUserIWalletCardsFilterRequest getUserIWalletCardsFilterRequest);
}
