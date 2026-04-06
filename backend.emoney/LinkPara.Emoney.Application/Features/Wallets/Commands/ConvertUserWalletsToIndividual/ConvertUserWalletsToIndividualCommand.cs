using LinkPara.Emoney.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.Emoney.Application.Features.Wallets.Commands.ConvertUserWalletsToIndividual;

public class ConvertUserWalletsToIndividualCommand : IRequest
{
    public Guid UserId { get; set; }
}

public class ConvertUserWalletsToIndividualCommandHandler : IRequestHandler<ConvertUserWalletsToIndividualCommand>
{
    private readonly IWalletService _walletService;

    public ConvertUserWalletsToIndividualCommandHandler(IWalletService walletService)
    {
        _walletService = walletService;
    }

    public async Task<Unit> Handle(ConvertUserWalletsToIndividualCommand request, CancellationToken cancellationToken)
    {
        await _walletService.ConvertUserWalletsToIndividualAsync(request);

        return Unit.Value;
    }
}