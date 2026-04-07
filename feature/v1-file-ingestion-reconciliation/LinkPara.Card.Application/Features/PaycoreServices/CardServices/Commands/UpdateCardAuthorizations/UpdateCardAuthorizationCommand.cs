using LinkPara.Card.Application.Commons.Interfaces;
using LinkPara.Card.Application.Commons.Models.PaycoreModels;
using MediatR;

namespace LinkPara.Card.Application.Features.PaycoreServices.CardServices.Commands.UpdateCardAuthorizations;

public class UpdateCardAuthorizationCommand : IRequest<PaycoreResponse>
{
    public string CardNumber { get; set; }
    public bool EcommercePermission { get; set; }
    public bool Non3DPermission { get; set; }
    public bool CashTransactionPermission { get; set; }
    public bool InternationalPermission { get; set; }
    public string ThreeDSecureType { get; set; }
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