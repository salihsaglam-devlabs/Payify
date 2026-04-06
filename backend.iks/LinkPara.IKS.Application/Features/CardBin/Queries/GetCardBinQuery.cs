using LinkPara.IKS.Application.Commons.Interfaces;
using LinkPara.IKS.Application.Commons.Models.IKSModels;
using LinkPara.IKS.Application.Commons.Models.IKSModels.CardBins.Request;
using LinkPara.IKS.Application.Commons.Models.IKSModels.CardBins.Response;
using MediatR;

namespace LinkPara.IKS.Application.Features.CardBin.Queries;

public class GetCardBinQuery : IRequest<IKSResponse<CardBinResponse>>
{
}

public class GetCardBinQueryHandler : IRequestHandler<GetCardBinQuery, IKSResponse<CardBinResponse>>
{
	private readonly ICardBinService _cardBinService;

	public GetCardBinQueryHandler(ICardBinService cardBinService)
	{
		_cardBinService = cardBinService;
	}

	public async Task<IKSResponse<CardBinResponse>> Handle(GetCardBinQuery request, CancellationToken cancellationToken)
	{
		return await _cardBinService.GetCardBinRangeAsync(new CardBinRequest());
	}
}
