
using LinkPara.CampaignManagement.Application.Features.IWalletLocations;

namespace LinkPara.CampaignManagement.Application.Commons.Interfaces;

public interface IIWalletLocationService
{
    Task<List<CityDto>> GetCitiesAsync();
    Task<List<TownDto>> GetTownsAsync(int cityId);
}
