using LinkPara.Card.Application.Commons.Interfaces;
using LinkPara.Card.Application.Commons.Models.PaycoreModels.CustomerModels;
using MediatR;

namespace LinkPara.Card.Application.Features.PaycoreServices.CustomerServices.Queries.GetCustomerInformation;
public class GetCustomerInformationQuery : IRequest<GetCustomerInformationResponse>
{
    public string BankingCustomerNo { get; set; }
}
public class GetCustomerInformationQueryHandler : IRequestHandler<GetCustomerInformationQuery, GetCustomerInformationResponse>
{
    private readonly IPaycoreCustomerService _paycoreCustomerService;
    public GetCustomerInformationQueryHandler(IPaycoreCustomerService paycoreCustomerService)
    {
        _paycoreCustomerService = paycoreCustomerService;
    }
    public async Task<GetCustomerInformationResponse> Handle(GetCustomerInformationQuery request, CancellationToken cancellationToken)
    {
        return await _paycoreCustomerService.GetCustomerInformationAsync(request);
    }
}
