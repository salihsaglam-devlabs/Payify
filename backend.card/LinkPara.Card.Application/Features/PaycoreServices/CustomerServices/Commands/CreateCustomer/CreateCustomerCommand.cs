using LinkPara.Card.Application.Commons.Interfaces;
using LinkPara.Card.Application.Commons.Models.PaycoreModels;
using LinkPara.Card.Application.Commons.Models.PaycoreModels.CustomerModels;
using MediatR;

namespace LinkPara.Card.Application.Features.PaycoreServices.CustomerServices.Commands.CreateCustomer;
public class CreateCustomerCommand : IRequest<PaycoreResponse>
{
    public string WalletNumber { get; set; }
    public string ProductCode { get; set; }
    public string CustomerGroupCode { get; set; }
    public string Name { get; set; }
    public string Surname { get; set; }
    public CustomerAddress[] CustomerAddresses { get; set; }
    public CustomerCommunication[] CustomerCommunications { get; set; }
    public CustomerLimit[] CustomerLimits { get; set; }
}
public class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, PaycoreResponse>
{
    private readonly IPaycoreCustomerService _paycoreService;
    public CreateCustomerCommandHandler(IPaycoreCustomerService paycoreService)
    {
        _paycoreService = paycoreService;
    }
    public async Task<PaycoreResponse> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        return await _paycoreService.CreateCustomerAsync(request);
    }
}