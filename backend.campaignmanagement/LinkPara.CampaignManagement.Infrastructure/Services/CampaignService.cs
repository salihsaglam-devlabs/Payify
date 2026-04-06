using LinkPara.CampaignManagement.Application.Commons.Interfaces;
using LinkPara.CampaignManagement.Application.Commons.Interfaces.HttpClients;
using LinkPara.CampaignManagement.Application.Commons.Models.Responses;

namespace LinkPara.CampaignManagement.Infrastructure.Services;

public class CampaignService : ICampaignService
{
    private readonly IIWalletHttpClient _iWalletHttpClient;
    public CampaignService(IIWalletHttpClient iWalletHttpClient)
    {
        _iWalletHttpClient = iWalletHttpClient;
    }

    public async Task<List<Campaign>> GetAllCampaignFromExternalAsync()
    {
        var result = await _iWalletHttpClient.GetCampaignsAsync();
        return result;
    }
}
