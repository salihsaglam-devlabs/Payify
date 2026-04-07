using LinkPara.Card.Application.Commons.Interfaces;
using LinkPara.Card.Application.Commons.Models.PaycoreModels.CardModels;
using MediatR;

namespace LinkPara.Card.Application.Features.PaycoreServices.CardServices.Queries.GetCardAuthorization;

public class GetCardAuthorizationsQuery : IRequest<GetCardAuthorizationsResponse>
{
    public string CardNumber { get; set; }
}
public class GetCardAuthorizationQueryHandler : IRequestHandler<GetCardAuthorizationsQuery, GetCardAuthorizationsResponse>
{
    private readonly IPaycoreCardService _paycoreService;
    public GetCardAuthorizationQueryHandler(IPaycoreCardService paycoreService)
    {
        _paycoreService = paycoreService;
    }
    public async Task<GetCardAuthorizationsResponse> Handle(GetCardAuthorizationsQuery request, CancellationToken cancellationToken)
    {
        return await _paycoreService.GetCardAuthorizationsAsync(request);
    }
}
