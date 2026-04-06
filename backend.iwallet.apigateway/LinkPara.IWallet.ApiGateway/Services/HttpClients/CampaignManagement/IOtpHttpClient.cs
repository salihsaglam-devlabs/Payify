using LinkPara.IWallet.ApiGateway.Models.Requests;
using LinkPara.IWallet.ApiGateway.Models.Responses;

namespace LinkPara.IWallet.ApiGateway.Services.HttpClients.CampaignManagement;

public interface IOtpHttpClient
{
    Task<BaseServiceResponse<SendIWalletSmsOtpResponse>> SendIWalletSmsOtpAsync(SendIWalletSmsOtpRequest request);
}
