using LinkPara.Card.Application.Commons.Interfaces;
using LinkPara.Card.Application.Commons.Models.PaycoreModels.CardModels;
using MediatR;

namespace LinkPara.Card.Application.Features.PaycoreServices.CardServices.Queries.GetCardSensitiveData;

public class GetCardStatusQuery : IRequest<GetCardStatusResponse>
{
    public string[] CardNos { get; set; }
}

public class GetCardStatusQueryHandler : IRequestHandler<GetCardStatusQuery, GetCardStatusResponse>
{
    private readonly IPaycoreCardService _paycoreCardService;

    public GetCardStatusQueryHandler(IPaycoreCardService paycoreCardService)
    {
        _paycoreCardService = paycoreCardService;
    }

    public async Task<GetCardStatusResponse> Handle(GetCardStatusQuery request, CancellationToken cancellationToken)
    {
        return await _paycoreCardService.GetCardStatusAsync(request);
    }
}