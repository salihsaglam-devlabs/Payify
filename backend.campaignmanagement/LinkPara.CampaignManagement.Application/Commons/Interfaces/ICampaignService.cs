using LinkPara.CampaignManagement.Application.Commons.Models.Responses;

namespace LinkPara.CampaignManagement.Application.Commons.Interfaces;

public interface ICampaignService
{
    Task<List<Campaign>> GetAllCampaignFromExternalAsync();
}
