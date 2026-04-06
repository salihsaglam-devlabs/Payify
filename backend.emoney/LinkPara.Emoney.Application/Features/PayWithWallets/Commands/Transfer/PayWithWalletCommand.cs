using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Application.Commons.Models;
using MediatR;

namespace LinkPara.Emoney.Application.Features.PayWithWallets.Commands.Transfer;

public class PayWithWalletCommand : IRequest<PayWithWalletResponse>
{
    public string PaymentReferenceId { get; set; }
    public string SenderPhoneNumber { get; set; }
    public string SenderWalletNumber { get; set; }
    public string ReceiverWalletNumber { get; set; }
    public string Description { get; set; }
    public decimal Amount { get; set; }
    public string UserId { get; set; }
    public string PartnerNumber { get; set; }
    public bool IsLoggedIn { get; set; }

}
public class PayWithWalletCommandHandler : IRequestHandler<PayWithWalletCommand, PayWithWalletResponse>
{
    private readonly IPayWithWalletService _transferService;

    public PayWithWalletCommandHandler(IPayWithWalletService transferService)
    {
        _transferService = transferService;
    }
    
    public async Task<PayWithWalletResponse> Handle(PayWithWalletCommand request, CancellationToken cancellationToken)
    {
        
        return await _transferService.PayWithWalletAsync(request, cancellationToken);
    }
}