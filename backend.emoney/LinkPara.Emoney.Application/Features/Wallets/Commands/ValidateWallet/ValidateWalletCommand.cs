using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Application.Commons.Models.WalletModels;
using MediatR;

namespace LinkPara.Emoney.Application.Features.Wallets.Commands.ValidateWallet;
public class ValidateWalletCommand : IRequest<ValidateWalletResponse>
{
    public string WalletNumber { get; set; }
    public string CurrencyCode { get; set; }
}

public class ValidateAndAuthorizeWalletCommandHandler : IRequestHandler<ValidateWalletCommand, ValidateWalletResponse>
{
    private readonly IWalletService _walletService;
    public ValidateAndAuthorizeWalletCommandHandler(IWalletService walletService)
    {
        _walletService = walletService;
    }
    public async Task<ValidateWalletResponse> Handle(ValidateWalletCommand request, CancellationToken cancellationToken)
    {
        return await _walletService.ValidateWalletAsync(request);
    }
}
