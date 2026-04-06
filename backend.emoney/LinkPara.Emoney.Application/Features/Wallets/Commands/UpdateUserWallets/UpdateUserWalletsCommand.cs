using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.SharedModels.Persistence;
using MediatR;

namespace LinkPara.Emoney.Application.Features.Wallets.Commands.UpdateUserWallets;

public class UpdateUserWalletsCommand : IRequest
{
    public Guid UserId { get; set; }
    public bool IsBlockage { get; set; }
    public RecordStatus RecordStatus { get; set; }
}

public class UpdateUserWalletsCommandHandler : IRequestHandler<UpdateUserWalletsCommand>
{
    private readonly IWalletService _walletService;

    public UpdateUserWalletsCommandHandler(IWalletService walletService)
    {
        _walletService = walletService;
    }

    public async Task<Unit> Handle(UpdateUserWalletsCommand request, CancellationToken cancellationToken)
    {
        await _walletService.UpdateUserWalletsAsync(request.UserId, request);

        return Unit.Value;
    }
}