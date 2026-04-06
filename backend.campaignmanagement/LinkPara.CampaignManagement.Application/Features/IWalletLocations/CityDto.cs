using LinkPara.CampaignManagement.Application.Commons.Mappings;
using LinkPara.CampaignManagement.Application.Commons.Models.Responses;

namespace LinkPara.CampaignManagement.Application.Features.IWalletLocations;

public class CityDto : IMapFrom<City>
{
    public int Id { get; set; }
    public string Name { get; set; }
}
