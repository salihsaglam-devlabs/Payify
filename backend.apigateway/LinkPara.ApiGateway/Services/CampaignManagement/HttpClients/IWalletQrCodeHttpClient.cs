using LinkPara.ApiGateway.Commons.Helpers;
using LinkPara.ApiGateway.Services.CampaignManagement.Models.Requests;
using LinkPara.ApiGateway.Services.CampaignManagement.Models.Responses;
using LinkPara.ApiGateway.Services.Emoney.HttpClients;
using LinkPara.ApiGateway.Services.Emoney.Models.Requests;
using System.Text.Json;

namespace LinkPara.ApiGateway.Services.CampaignManagement.HttpClients;

public class IWalletQrCodeHttpClient : HttpClientBase, IIWalletQrCodeHttpClient
{
    private readonly IWalletHttpClient _emoneyWalletHttpClient;
    private readonly IServiceRequestConverter _serviceRequestConverter;

    public IWalletQrCodeHttpClient(HttpClient client, 
        IHttpContextAccessor httpContextAccessor, 
        IWalletHttpClient emoneyWalletHttpClient, 
        IServiceRequestConverter serviceRequestConverter) : base(client, httpContextAccessor)
    {
        _emoneyWalletHttpClient = emoneyWalletHttpClient;
        _serviceRequestConverter = serviceRequestConverter;
    }

    public async Task<IWalletQrCodeResponse> GenerateQrCodeAsync(IWalletGenerateQrCodeRequest request)
    {
        var clientRequest = _serviceRequestConverter.Convert<IWalletGenerateQrCodeRequest, IWalletGenerateQrCodeServiceRequest>(request);

        var wallets = await _emoneyWalletHttpClient.GetUserWalletsAsync(new GetUserWalletsFilterRequest { });
        var mainWallet = wallets.FirstOrDefault(s => s.IsMainWallet);

        clientRequest.WalletNumber = mainWallet.WalletNumber;

        var response = await PostAsJsonAsync<IWalletGenerateQrCodeServiceRequest>($"v1/IWalletQrCodes", clientRequest);

        var responseString = await response.Content.ReadAsStringAsync();
        var qrCode = JsonSerializer.Deserialize<IWalletQrCodeResponse>(responseString, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return qrCode ?? throw new InvalidOperationException();
    }
}
