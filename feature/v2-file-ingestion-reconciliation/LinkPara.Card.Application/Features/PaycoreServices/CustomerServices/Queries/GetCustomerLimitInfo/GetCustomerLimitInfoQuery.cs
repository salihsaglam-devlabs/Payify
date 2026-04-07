using LinkPara.Card.Application.Commons.Interfaces;
using LinkPara.Card.Application.Commons.Models.PaycoreModels.CustomerModels;
using MediatR;

namespace LinkPara.Card.Application.Features.PaycoreServices.CustomerServices.Queries.GetCustomerLimitInfo;

public class GetCustomerLimitInfoQuery : IRequest<List<GetCustomerLimitInfoResponse>>
{
    public string BankingCustomerNo { get; set; }
}

public class GetCustomerLimitInfoQueryHandler : IRequestHandler<GetCustomerLimitInfoQuery, List<GetCustomerLimitInfoResponse>>
{
    private readonly IPaycoreCustomerService _paycoreCustomerService;

    public GetCustomerLimitInfoQueryHandler(IPaycoreCustomerService paycoreCustomerService)
    {
        _paycoreCustomerService = paycoreCustomerService;
    }

    public async Task<List<GetCustomerLimitInfoResponse>> Handle(GetCustomerLimitInfoQuery request, CancellationToken cancellationToken)
    {
        return await _paycoreCustomerService.GetCustomerLimitInfoAsync(request);
    }
}
