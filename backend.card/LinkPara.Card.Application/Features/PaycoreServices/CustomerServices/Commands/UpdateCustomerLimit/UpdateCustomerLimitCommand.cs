using LinkPara.Card.Application.Commons.Interfaces;
using LinkPara.Card.Application.Commons.Models.PaycoreModels;
using MediatR;

namespace LinkPara.Card.Application.Features.PaycoreServices.CustomerServices.Commands.UpdateCustomerLimit;

public class UpdateCustomerLimitCommand : IRequest<PaycoreResponse>
{
    public string BankingCustomerNo { get; set; }
    public int NewLimit { get; set; }
    public string MemoText { get; set; }
    public string LimitAssignType { get; set; }
    public int CurrencyCode { get; set; }
    public bool IsLimitUsedControl { get; set; }
}

public class UpdateCustomerLimitCommandHandler : IRequestHandler<UpdateCustomerLimitCommand, PaycoreResponse>
{
    private readonly IPaycoreCustomerService _paycoreCustomerService;

    public UpdateCustomerLimitCommandHandler(IPaycoreCustomerService paycoreCustomerService)
    {
        _paycoreCustomerService = paycoreCustomerService;
    }

    public async Task<PaycoreResponse> Handle(UpdateCustomerLimitCommand request, CancellationToken cancellationToken)
    {
        return await _paycoreCustomerService.UpdateCustomerLimitAsync(request);
    }
}
