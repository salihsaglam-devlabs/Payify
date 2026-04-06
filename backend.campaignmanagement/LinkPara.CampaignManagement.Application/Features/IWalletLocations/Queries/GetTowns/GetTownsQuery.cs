using LinkPara.CampaignManagement.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.CampaignManagement.Application.Features.IWalletLocations.Queries.GetTowns;

public class GetTownsQuery : IRequest<List<TownDto>>
{
    public int CityId { get; set; }
}

public class GetTownsQueryHandler : IRequestHandler<GetTownsQuery, List<TownDto>>
{
    private readonly IIWalletLocationService _iwalletLocationService;

    public GetTownsQueryHandler(IIWalletLocationService iwalletLocationService)
    {
        _iwalletLocationService = iwalletLocationService;
    }

    public async Task<List<TownDto>> Handle(GetTownsQuery request, CancellationToken cancellationToken)
    {
        return await _iwalletLocationService.GetTownsAsync(request.CityId);
    }
}