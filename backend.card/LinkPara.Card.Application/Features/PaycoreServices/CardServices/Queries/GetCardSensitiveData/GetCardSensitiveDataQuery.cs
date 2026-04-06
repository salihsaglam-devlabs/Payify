using LinkPara.Card.Application.Commons.Interfaces;
using LinkPara.Card.Application.Commons.Models.PaycoreModels.CardModels;
using MediatR;

namespace LinkPara.Card.Application.Features.PaycoreServices.CardServices.Queries.GetCardSensitiveData;

public class GetCardSensitiveDataQuery : IRequest<GetCardSensitiveDataResponse>
{
    public string TokenPan { get; set; }
}

public class GetCardSensitiveDataQueryHandler : IRequestHandler<GetCardSensitiveDataQuery, GetCardSensitiveDataResponse>
{
    private readonly IPaycoreCardService _paycoreCardService;

    public GetCardSensitiveDataQueryHandler(IPaycoreCardService paycoreCardService)
    {
        _paycoreCardService = paycoreCardService;
    }

    public async Task<GetCardSensitiveDataResponse> Handle(GetCardSensitiveDataQuery request, CancellationToken cancellationToken)
    {
        return await _paycoreCardService.GetCardSensitiveDataAsync(request);
    }
}