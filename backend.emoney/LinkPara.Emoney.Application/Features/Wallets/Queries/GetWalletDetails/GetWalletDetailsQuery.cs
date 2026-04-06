using LinkPara.Emoney.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.Emoney.Application.Features.Wallets.Queries.GetWalletDetails;

public class GetWalletDetailsQuery : IRequest<WalletDto>
{
    public Guid UserId { get; set; }
    public Guid WalletId { get; set; }
}

public class GetWalletDetailsQueryHandler : IRequestHandler<GetWalletDetailsQuery, WalletDto>
{
    private readonly IWalletService _walletService;

    public GetWalletDetailsQueryHandler(IWalletService walletService)
    {
        _walletService = walletService;
    }

    public async Task<WalletDto> Handle(GetWalletDetailsQuery request, CancellationToken cancellationToken)
    {
        return await _walletService.GetWalletDetailsAsync(request);
    }
}