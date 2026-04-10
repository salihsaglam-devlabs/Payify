using LinkPara.Card.Application.Commons.Interfaces;
using LinkPara.Card.Application.Commons.Models.PaycoreModels;
using MediatR;

namespace LinkPara.Card.Application.Features.PaycoreServices.CardServices.Commands.UpdateCardStatus;

public class UpdateCardStatusCommand : IRequest<PaycoreResponse>
{
    public string CardNumber { get; set; }
    public string StatusCode { get; set; }
    public string Description { get; set; }
    public string StatusReasonCode { get; set; }
}

public class UpdateCardStatusCommandHandler : IRequestHandler<UpdateCardStatusCommand, PaycoreResponse>
{
    private readonly IPaycoreCardService _paycoreService;
    public UpdateCardStatusCommandHandler(IPaycoreCardService paycoreService)
    {
        _paycoreService = paycoreService;
    }
    public Task<PaycoreResponse> Handle(UpdateCardStatusCommand request, CancellationToken cancellationToken)
    {
        return _paycoreService.UpdateCardStatusAsync(request);
    }
}