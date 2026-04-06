using LinkPara.CampaignManagement.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.CampaignManagement.Application.Features.IWalletCards.Queries.GetCard;

public class GetCardQuery : IRequest<IWalletCardDto>
{
    public Guid UserId { get; set; }
}

public class GetCardQueryHandler : IRequestHandler<GetCardQuery, IWalletCardDto>
{
    private readonly IIWalletCardService _walletCardService;

    public GetCardQueryHandler(IIWalletCardService walletCardService)
    {
        _walletCardService = walletCardService;
    }

    public async Task<IWalletCardDto> Handle(GetCardQuery request, CancellationToken cancellationToken)
    {
        return await _walletCardService.GetUserCardAsync(request);
    }
}
