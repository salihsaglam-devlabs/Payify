using LinkPara.Card.Application.Commons.Interfaces;
using LinkPara.Card.Application.Commons.Models.PaycoreModels.CustomerModels;
using MediatR;

namespace LinkPara.Card.Application.Features.PaycoreServices.CustomerServices.Queries.GetCustomerCards;

public class GetCustomerCardsQuery : IRequest<List<GetCustomerCardsResponse>>
{
    public string BankingCustomerNo { get; set; }
}

public class GetCustomerCardsQueryHandler : IRequestHandler<GetCustomerCardsQuery, List<GetCustomerCardsResponse>>
{
    private readonly IPaycoreCustomerService _paycoreCustomerService;

    public GetCustomerCardsQueryHandler(IPaycoreCustomerService paycoreCustomerService)
    {
        _paycoreCustomerService = paycoreCustomerService;
    }

    public async Task<List<GetCustomerCardsResponse>> Handle(GetCustomerCardsQuery request, CancellationToken cancellationToken)
    {
        return await _paycoreCustomerService.GetCustomerCardsAsync(request);
    }
}
