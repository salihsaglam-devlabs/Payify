using LinkPara.Card.Application.Commons.Interfaces;
using LinkPara.Card.Application.Commons.Models.PaycoreModels.CardModels;
using MediatR;

namespace LinkPara.Card.Application.Features.PaycoreServices.CardServices.Queries.GetCardInformation; 
public class GetCardInformationsQuery : IRequest<GetCardInformationsResponse>
{
    public string CardNumber { get; set; }
}
public class GetCardInformationCommandHandler : IRequestHandler<GetCardInformationsQuery, GetCardInformationsResponse>
{
    private readonly IPaycoreCardService _paycoreService;
    public GetCardInformationCommandHandler(IPaycoreCardService paycoreService)
    {
        _paycoreService = paycoreService;
    }
    public async Task<GetCardInformationsResponse> Handle(GetCardInformationsQuery request, CancellationToken cancellationToken)
    {
        return await _paycoreService.GetCardInformationsAsync(request);
    }
}
