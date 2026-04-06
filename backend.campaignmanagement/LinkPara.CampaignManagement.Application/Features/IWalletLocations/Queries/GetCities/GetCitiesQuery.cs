
using LinkPara.CampaignManagement.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.CampaignManagement.Application.Features.IWalletLocations.Queries.GetCities;

public class GetCitiesQuery : IRequest<List<CityDto>>
{
}

public class GetCitiesQueryHandler : IRequestHandler<GetCitiesQuery, List<CityDto>>
{
    private readonly IIWalletLocationService _iwalletLocationService;

    public GetCitiesQueryHandler(IIWalletLocationService iwalletLocationService)
    {
        _iwalletLocationService = iwalletLocationService;
    }

    public async Task<List<CityDto>> Handle(GetCitiesQuery request, CancellationToken cancellationToken)
    {
        return await _iwalletLocationService.GetCitiesAsync();
    }
}