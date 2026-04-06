using LinkPara.Emoney.Application.Features.PayWithWallets;
using LinkPara.Emoney.Application.Features.PayWithWallets.Commands.Transfer;
using LinkPara.Emoney.Application.Features.PayWithWallets.Commands.TransferForLoggedInUser;

namespace LinkPara.Emoney.Application.Commons.Interfaces;

public interface IPayWithWalletService
{
    Task<PayWithWalletResponse> PayWithWalletAsync(PayWithWalletCommand request, CancellationToken cancellationToken);
    Task<PayWithWalletResponse> TransferForLoggedInUserAsync(TransferForLoggedInUserCommand request, CancellationToken cancellationToken);
}