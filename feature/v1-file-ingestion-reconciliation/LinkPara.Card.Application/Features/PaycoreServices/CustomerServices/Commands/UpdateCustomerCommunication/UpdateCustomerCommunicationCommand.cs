using LinkPara.Card.Application.Commons.Interfaces;
using LinkPara.Card.Application.Commons.Models.PaycoreModels;
using LinkPara.Card.Application.Commons.Models.PaycoreModels.CustomerModels;
using MediatR;

namespace LinkPara.Card.Application.Features.PaycoreServices.CustomerServices.Commands.UpdateCustomerCommunication;

public class UpdateCustomerCommunicationCommand : IRequest<PaycoreResponse>
{
    public string BankingCustomerNo { get; set; }
    public CustomerCommunication[] CustomerCommunications { get; set; }
}

public class UpdateCustomerCommunicationCommandHandler : IRequestHandler<UpdateCustomerCommunicationCommand, PaycoreResponse>
{
    private readonly IPaycoreCustomerService _paycoreCustomerService;

    public UpdateCustomerCommunicationCommandHandler(IPaycoreCustomerService paycoreCustomerService)
    {
        _paycoreCustomerService = paycoreCustomerService;
    }

    public async Task<PaycoreResponse> Handle(UpdateCustomerCommunicationCommand request, CancellationToken cancellationToken)
    {
        return await _paycoreCustomerService.UpdateCustomerCommunicationAsync(request);
    }
}
