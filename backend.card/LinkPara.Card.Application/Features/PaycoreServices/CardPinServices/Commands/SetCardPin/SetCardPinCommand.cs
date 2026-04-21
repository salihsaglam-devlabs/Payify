using LinkPara.Card.Application.Commons.Interfaces;
using LinkPara.Card.Application.Commons.Models.PaycoreModels.SecurityModels;
using MediatR;

namespace LinkPara.Card.Application.Features.PaycoreServices.CardPinServices.Commands.SetCardPin;
public class SetCardPinCommand : IRequest<EncDecPinblockResponse>
{
    public string TokenPan { get; set; }
    public string ClearPin { get; set; }
}
public class SetCardPinCommandHandler : IRequestHandler<SetCardPinCommand, EncDecPinblockResponse>
{
    private readonly IPaycoreSecurityService _paycoreSecurityService;
    public SetCardPinCommandHandler(IPaycoreSecurityService paycoreSecurityService)
    {
        _paycoreSecurityService = paycoreSecurityService;
    }
    public async Task<EncDecPinblockResponse> Handle(SetCardPinCommand command, CancellationToken cancellationToken)
    {
        return await _paycoreSecurityService.SetCardPinAsync(command);
    }
}