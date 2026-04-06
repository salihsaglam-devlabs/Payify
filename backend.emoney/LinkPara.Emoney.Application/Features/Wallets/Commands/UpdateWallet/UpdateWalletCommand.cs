using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.SharedModels.Persistence;
using MediatR;

namespace LinkPara.Emoney.Application.Features.Wallets.Commands.UpdateWallet;

public class UpdateWalletCommand : IRequest
{
    public Guid UserId { get; set; }
    public Guid WalletId { get; set; }
    public string FriendlyName { get; set; }
    public RecordStatus RecordStatus { get; set; }
    public bool IsBlocked { get; set; }
}

public class UpdateWalletCommandHandler : IRequestHandler<UpdateWalletCommand>
{
    private readonly IWalletService _walletService;

    public UpdateWalletCommandHandler(IWalletService walletService)
    {
        _walletService = walletService;
    }

    public async Task<Unit> Handle(UpdateWalletCommand request, CancellationToken cancellationToken)
    {
        await _walletService.UpdateWalletAsync(request, cancellationToken);

        return Unit.Value;
    }
}