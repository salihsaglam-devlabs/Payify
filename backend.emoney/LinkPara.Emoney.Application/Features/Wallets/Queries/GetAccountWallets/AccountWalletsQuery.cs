using LinkPara.Emoney.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.Emoney.Application.Features.Wallets.Queries.GetAccountWallets;

public class AccountWalletsQuery : IRequest<List<WalletDto>>
{
    public Guid AccountId { get; set; }
}

public class AccountWalletsQueryyHandler : IRequestHandler<AccountWalletsQuery, List<WalletDto>>
{
    private readonly IWalletService _walletService;

    public AccountWalletsQueryyHandler(IWalletService walletService)
    {
        _walletService = walletService;
    }

    public async Task<List<WalletDto>> Handle(AccountWalletsQuery query, CancellationToken cancellationToken)
    {
        return await _walletService.GetAccountWalletsAsync(query);
    }
}