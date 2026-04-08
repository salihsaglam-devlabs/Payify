using LinkPara.Card.Application.Commons.Interfaces;
using LinkPara.Card.Application.Commons.Models.WalletModels.CardModels;
using MediatR;

namespace LinkPara.Card.Application.Features.WalletServices.CardServices.Queries.GetCustomerWalletCardsQuery;

public class GetCustomerWalletCardsQuery : IRequest<List<CustomerWalletCardDto>>
{
    public string WalletNumber { get; set; }
}
public class GetCustomerWalletCardsQueryHandler : IRequestHandler<GetCustomerWalletCardsQuery, List<CustomerWalletCardDto>>
{
    private readonly ICustomerWalletCardService _walletService;
    public GetCustomerWalletCardsQueryHandler(ICustomerWalletCardService walletService)
    {
        _walletService = walletService;
    }
    public async Task<List<CustomerWalletCardDto>> Handle(GetCustomerWalletCardsQuery request, CancellationToken cancellationToken)
    {
        return await _walletService.GetCustomerWalletCardsAsync(request);
    }
}
