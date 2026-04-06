using LinkPara.Emoney.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.Emoney.Application.Features.Wallets.Queries.GetWalletDetails;

public class GetWalletDetailsPartnerQuery : IRequest<WalletPartnerDto>
{
    public Guid UserId { get; set; }
    public Guid WalletId { get; set; }
    public string Msisdn { get; set; }
}

public class GetWalletDetailsPartnerQueryHandler : IRequestHandler<GetWalletDetailsPartnerQuery, WalletPartnerDto>
{
    private readonly IWalletService _walletService;

    public GetWalletDetailsPartnerQueryHandler(IWalletService walletService)
    {
        _walletService = walletService;
    }

    public async Task<WalletPartnerDto> Handle(GetWalletDetailsPartnerQuery request, CancellationToken cancellationToken)
    {
        return await _walletService.GetWalletDetailsPartnerAsync(request);
    }
}