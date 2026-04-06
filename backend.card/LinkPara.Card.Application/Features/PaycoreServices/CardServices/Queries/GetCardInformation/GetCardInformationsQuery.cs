using LinkPara.Card.Application.Commons.Interfaces;
using LinkPara.Card.Application.Commons.Models.PaycoreModels.CardModels;
using MediatR;

namespace LinkPara.Card.Application.Features.PaycoreServices.CardServices.Queries.GetCardInformation;
public class GetCardInformationsQuery : IRequest<List<GetCardInformationsResponse>>
{
    public string TokenPan { get; set; } 
}
public class GetCardInformationsQueryHandler : IRequestHandler<GetCardInformationsQuery, List<GetCardInformationsResponse>>
{
    private readonly IPaycoreCardService _paycoreService;
    public GetCardInformationsQueryHandler(IPaycoreCardService paycoreService)
    {
        _paycoreService = paycoreService;
    }

    public async Task<List<GetCardInformationsResponse>> Handle(GetCardInformationsQuery request, CancellationToken cancellationToken)
    {
        return await _paycoreService.GetCardInformationsAsync(request);
    }
}