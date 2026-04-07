using LinkPara.Card.Application.Commons.Interfaces;
using LinkPara.Card.Application.Commons.Models.PaycoreModels;
using LinkPara.Card.Application.Commons.Models.PaycoreModels.CustomerModels;
using MediatR;

namespace LinkPara.Card.Application.Features.PaycoreServices.CustomerServices.Commands.UpdateCustomerAddress;

public class UpdateCustomerAddressCommand : IRequest<PaycoreResponse>
{
    public string BankingCustomerNo { get; set; }
    public CustomerAddress[] CustomerAddresses { get; set; }
}

public class UpdateCustomerAddressCommandHandler : IRequestHandler<UpdateCustomerAddressCommand, PaycoreResponse>
{
    private readonly IPaycoreCustomerService _paycoreCustomerService;

    public UpdateCustomerAddressCommandHandler(IPaycoreCustomerService paycoreCustomerService)
    {
        _paycoreCustomerService = paycoreCustomerService;
    }

    public async Task<PaycoreResponse> Handle(UpdateCustomerAddressCommand request, CancellationToken cancellationToken)
    {
        return await _paycoreCustomerService.UpdateCustomerAddressAsync(request);
    }
}
