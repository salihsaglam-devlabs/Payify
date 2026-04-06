using LinkPara.Card.Application.Commons.Interfaces;
using LinkPara.Card.Application.Commons.Models.PaycoreModels.CardModels;
using MediatR;

namespace LinkPara.Card.Application.Features.PaycoreServices.CardServices.Queries.GetClearCardNo;

public class GetClearCardNoQuery : IRequest<List<GetClearCardNoResponse>>
{
    public string [] Cards { get; set; }
}

public class GetClearCardNoQueryHandler : IRequestHandler<GetClearCardNoQuery, List<GetClearCardNoResponse>>
{
    private readonly IPaycoreCardService _paycoreCardService;

    public GetClearCardNoQueryHandler(IPaycoreCardService paycoreCardService)
    {
        _paycoreCardService = paycoreCardService;
    }

    public async Task<List<GetClearCardNoResponse>> Handle(GetClearCardNoQuery request, CancellationToken cancellationToken)
    {
        return await _paycoreCardService.GetClearCardNoAsync(request);
    }
}