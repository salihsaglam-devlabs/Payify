using LinkPara.Card.Application.Commons.Interfaces;
using LinkPara.Card.Application.Commons.Models.PaycoreModels.CardModels;
using MediatR;

namespace LinkPara.Card.Application.Features.PaycoreServices.CardServices.Queries.GetCardTransactions;

public class GetCardLastCourierActivityQuery : IRequest<GetCardLastCourierActivityResponse>
{
    public string CardNumber { get; set; }
    public string BankingCustomerNumber { get; set; }
    public string BarcodeNumber { get; set; }
    public string BatchBarcodeNumber { get; set; }
    public string CustomerNumber { get; set; }
    public string DCI { get; set; }
    public string ProductCode { get; set; }
}
public class GetCardLastCourierActivityQueryHandler : IRequestHandler<GetCardLastCourierActivityQuery, GetCardLastCourierActivityResponse>
{
    private readonly IPaycoreCardService _paycoreService;
    public GetCardLastCourierActivityQueryHandler(IPaycoreCardService paycoreService)
    {
        _paycoreService = paycoreService;
    }

    public async Task<GetCardLastCourierActivityResponse> Handle(GetCardLastCourierActivityQuery request, CancellationToken cancellationToken)
    {
        return await _paycoreService.GetCardLastCorierActivityAsync(request);
    }
}
