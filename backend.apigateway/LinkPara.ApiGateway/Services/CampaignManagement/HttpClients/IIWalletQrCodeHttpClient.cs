using LinkPara.ApiGateway.Services.CampaignManagement.Models.Requests;
using LinkPara.ApiGateway.Services.CampaignManagement.Models.Responses;

namespace LinkPara.ApiGateway.Services.CampaignManagement.HttpClients
{
    public interface IIWalletQrCodeHttpClient
    {
        Task<IWalletQrCodeResponse> GenerateQrCodeAsync(IWalletGenerateQrCodeRequest request);
    }
}
