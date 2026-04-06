using LinkPara.Card.Application.Commons.Interfaces;
using LinkPara.Card.Application.Commons.Models.PaycoreModels;
using LinkPara.Card.Application.Commons.Models.PaycoreModels.CardModels;
using MediatR;

namespace LinkPara.Card.Application.Features.PaycoreServices.CardServices.Commands.UpdateCardAuthorizations;

public class UpdateCardAuthorizationCommand : IRequest<PaycoreResponse>
{
    public string CardNumber { get; set; }
    public CrdCardAuth CrdCardAuth { get; set; }
}
public class UpdateCardAuthorizationCommandHandler : IRequestHandler<UpdateCardAuthorizationCommand, PaycoreResponse>
{
    private readonly IPaycoreCardService _paycoreService;
    public UpdateCardAuthorizationCommandHandler(IPaycoreCardService paycoreService)
    {
        _paycoreService = paycoreService;
    }
    public async Task<PaycoreResponse> Handle(UpdateCardAuthorizationCommand request, CancellationToken cancellationToken)
    {
        return await _paycoreService.UpdateCardAuthorizationsAsync(request);
    }
}