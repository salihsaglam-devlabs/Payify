using LinkPara.Card.Application.Commons.Interfaces;
using LinkPara.Card.Application.Commons.Models.PaycoreModels.SecurityModels;
using MediatR;

namespace LinkPara.Card.Application.Features.PaycoreServices.CardPinServices.Commands.SetCardPin;
public class SetCardPinCommand : IRequest<SetCardBinResponse>
{
    public string TokenPan { get; set; }
    public string ClearPin { get; set; }
}
public class SetCardPinCommandHandler : IRequestHandler<SetCardPinCommand, SetCardBinResponse>
{
    private readonly IPaycoreSecurityService _paycoreSecurityService;
    public SetCardPinCommandHandler(IPaycoreSecurityService paycoreSecurityService)
    {
        _paycoreSecurityService = paycoreSecurityService;
    }
    public async Task<SetCardBinResponse> Handle(SetCardPinCommand command, CancellationToken cancellationToken)
    {
        return await _paycoreSecurityService.SetCardPinAsync(command);
    }
}