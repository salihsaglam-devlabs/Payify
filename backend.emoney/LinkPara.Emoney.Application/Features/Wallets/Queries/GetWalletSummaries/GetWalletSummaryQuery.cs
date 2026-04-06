using LinkPara.Emoney.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.Emoney.Application.Features.Wallets.Queries.GetWalletSummaries;

public class GetWalletSummaryQuery : IRequest<WalletSummaryDto>
{
    public Guid UserId { get; set; }
    public string WalletNumber { get; set; }
}
public class GetWalletSummariesQueryHandler : IRequestHandler<GetWalletSummaryQuery, WalletSummaryDto>
{
    private readonly IWalletService _walletService;

    public GetWalletSummariesQueryHandler(IWalletService walletService)
    {
        _walletService = walletService;
    }

    public async Task<WalletSummaryDto> Handle(GetWalletSummaryQuery request, CancellationToken cancellationToken)
    {
        return await _walletService.GetWalletSummaryAsync(request);
    }
}
