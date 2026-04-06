using AutoMapper;
using LinkPara.CampaignManagement.Application.Commons.Interfaces;
using LinkPara.CampaignManagement.Application.Commons.Interfaces.HttpClients;
using LinkPara.CampaignManagement.Application.Features.IWalletLocations;

namespace LinkPara.CampaignManagement.Infrastructure.Services;

public class IWalletLocationService : IIWalletLocationService
{
    private readonly IIWalletHttpClient _iwalletHttpClient;
    private readonly IMapper _mapper;

    public IWalletLocationService(IIWalletHttpClient iwalletHttpClient, IMapper mapper)
    {
        _iwalletHttpClient = iwalletHttpClient;
        _mapper = mapper;
    }

    public async Task<List<CityDto>> GetCitiesAsync()
    {
        var cities = await _iwalletHttpClient.GetCitiesAsync();
        return _mapper.Map<List<CityDto>>(cities);
    }

    public async Task<List<TownDto>> GetTownsAsync(int cityId)
    {
        var towns = await _iwalletHttpClient.GetTownsAsync(cityId);
        return _mapper.Map<List<TownDto>>(towns);
    }
}
