using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Application.Commons.Models;
using MediatR;

namespace LinkPara.Emoney.Application.Features.Wallets.Queries.GetWalletDetails;

public class GetMoneyTransferPaymentTypeQuery : IRequest<List<MoneyTransferPaymentType>>
{
    
}

public class GetMoneyTransferPaymentTypeQueryHandler : IRequestHandler<GetMoneyTransferPaymentTypeQuery, List<MoneyTransferPaymentType>>
{
    private readonly IWalletService _walletService;

    public GetMoneyTransferPaymentTypeQueryHandler(IWalletService walletService)
    {
        _walletService = walletService;
    }

    public async Task<List<MoneyTransferPaymentType>> Handle(GetMoneyTransferPaymentTypeQuery request, CancellationToken cancellationToken)
    {
        return await _walletService.GetMoneyTransferPaymentTypeAsync();
    }
}
