using LinkPara.Emoney.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.Emoney.Application.Features.PayWithWallets.Commands.TransferForLoggedInUser;

public class TransferForLoggedInUserCommand : IRequest<PayWithWalletResponse>
{
    public string PaymentReferenceId { get; set; }
    public string SenderPhoneNumber { get; set; }
    public string SenderWalletNumber { get; set; }
    public string ReceiverWalletNumber { get; set; }
    public string Description { get; set; }
    public decimal Amount { get; set; }
    public string UserId { get; set; }
    public string PartnerNumber { get; set; }

}
public class TransferForLoggedInUserCommandHandler : IRequestHandler<TransferForLoggedInUserCommand, PayWithWalletResponse>
{
    private readonly IPayWithWalletService _transferService;

    public TransferForLoggedInUserCommandHandler(IPayWithWalletService transferService)
    {
        _transferService = transferService;
    }
    
    public async Task<PayWithWalletResponse> Handle(TransferForLoggedInUserCommand request, CancellationToken cancellationToken)
    {
        
        return await _transferService.TransferForLoggedInUserAsync(request, cancellationToken);
    }
}