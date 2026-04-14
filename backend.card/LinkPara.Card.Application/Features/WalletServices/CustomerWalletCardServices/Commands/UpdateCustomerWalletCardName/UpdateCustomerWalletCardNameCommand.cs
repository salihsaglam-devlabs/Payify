using LinkPara.Card.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.Card.Application.Features.WalletServices.CardServices.Commands.UpdateCustomerWalletCardName;

public class UpdateCustomerWalletCardNameCommand : IRequest
{
    public string CardNumber { get; set; }
    public string CardName { get; set; }
}

public class UpdateCustomerWalletCardNameCommandHandler : IRequestHandler<UpdateCustomerWalletCardNameCommand>
{
    private readonly ICustomerWalletCardService _walletService;

    public UpdateCustomerWalletCardNameCommandHandler(ICustomerWalletCardService walletService)
    {
        _walletService = walletService;
    }
    public async Task<Unit> Handle(UpdateCustomerWalletCardNameCommand request, CancellationToken cancellationToken)
    {
        await _walletService.UpdateCustomerWalletCardNameAsync(request);
        return Unit.Value;
    }
}
