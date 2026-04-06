using LinkPara.ApiGateway.Services.CampaignManagement.Models.Responses;

namespace LinkPara.ApiGateway.Services.CampaignManagement.HttpClients;

public interface ICampaignHttpClient
{
    Task<List<CampaignResponse>> GetCampaignsAsync();
}
