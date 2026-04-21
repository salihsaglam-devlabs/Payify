using LinkPara.Card.Application.Commons.Interfaces;
using LinkPara.Card.Application.Commons.Models.PaycoreModels.CardModels;
using MediatR;

namespace LinkPara.Card.Application.Features.PaycoreServices.CardServices.Queries.GetCardEmbossStatus;

public class GetCardEmbossStatusQuery : IRequest<GetCardEmbossStatusResponse>
{
    public string CardNo { get; set; }
}

public class GetCardEmbossStatusQueryHandler : IRequestHandler<GetCardEmbossStatusQuery, GetCardEmbossStatusResponse>
{
    private readonly IPaycoreCardService _paycoreCardService;

    public GetCardEmbossStatusQueryHandler(IPaycoreCardService paycoreCardService)
    {
        _paycoreCardService = paycoreCardService;
    }

    public async Task<GetCardEmbossStatusResponse> Handle(GetCardEmbossStatusQuery request, CancellationToken cancellationToken)
    {
        return await _paycoreCardService.GetCardEmbossStatusAsync(request);
    }
}
