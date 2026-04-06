using LinkPara.Emoney.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.Emoney.Application.Features.Wallets.Queries.GetUserWallets;

public class GetUserWalletsFilterQuery : IRequest<List<WalletDto>>
{
    public Guid UserId { get; set; }
}

public class GetUserWalletsQueryHandler : IRequestHandler<GetUserWalletsFilterQuery, List<WalletDto>>
{
    private readonly IWalletService _walletService;

    public GetUserWalletsQueryHandler(IWalletService walletService)
    {
        _walletService = walletService;
    }

    public async Task<List<WalletDto>> Handle(GetUserWalletsFilterQuery query, CancellationToken cancellationToken)
    {
        return await _walletService.GetUserWalletsAsync(query);
    }
}