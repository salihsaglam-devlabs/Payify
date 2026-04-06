using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Domain.Enums;
using MediatR;

namespace LinkPara.Emoney.Application.Features.Wallets.Commands.SaveWallet;

public class SaveWalletCommand : IRequest
{
    public string FriendlyName { get; set; }
    public Guid UserId { get; set; }
    public string CurrencyCode { get; set; }
    public CurrencyType CurrencyType { get; set; }
    public bool IsMainWallet { get; set; }
}

public class SaveWalletCommandHandler : IRequestHandler<SaveWalletCommand>
{
    private readonly IWalletService _walletService;

    public SaveWalletCommandHandler(IWalletService walletService)
    {
        _walletService = walletService;
    }

    public async Task<Unit> Handle(SaveWalletCommand request, CancellationToken cancellationToken)
    {
        await _walletService.SaveWalletAsync(request);

        return Unit.Value;
    }
}
