using LinkPara.Card.Application.Commons.Interfaces;
using LinkPara.Card.Application.Commons.Models.PaycoreModels;
using LinkPara.Card.Application.Commons.Models.PaycoreModels.CustomerModels;
using MediatR;

namespace LinkPara.Card.Application.Features.PaycoreServices.CustomerServices.Commands.UpdateCustomer;

public class UpdateCustomerCommand : IRequest<PaycoreResponse>
{
    public string BankingCustomerNo { get; set; }
    public string CustomerGroupCode { get; set; }
    public string Name { get; set; }
    public string Surname { get; set; }
}

public class UpdateCustomerCommandHandler : IRequestHandler<UpdateCustomerCommand, PaycoreResponse>
{
    private readonly IPaycoreCustomerService _paycoreCustomerService;

    public UpdateCustomerCommandHandler(IPaycoreCustomerService paycoreCustomerService)
    {
        _paycoreCustomerService = paycoreCustomerService;
    }

    public async Task<PaycoreResponse> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
    {
        return await _paycoreCustomerService.UpdateCustomerAsync(request);
    }
}