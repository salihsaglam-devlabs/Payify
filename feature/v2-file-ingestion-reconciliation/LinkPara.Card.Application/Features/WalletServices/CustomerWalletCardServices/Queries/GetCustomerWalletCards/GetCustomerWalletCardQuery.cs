using LinkPara.Card.Application.Commons.Interfaces;
using LinkPara.Card.Application.Commons.Models.WalletMoodels.CardModels;
using MediatR;

namespace LinkPara.Card.Application.Features.WalletServices.CardServices.Queries.GetCustomerWalletCardsQuery;

public class GetCustomerWalletCardsQuery : IRequest<GetCustomerWalletCardsResponse>
{
    public string CustomerNumber { get; set; }
}
public class GetCustomerWalletCardsQueryHandler : IRequestHandler<GetCustomerWalletCardsQuery, GetCustomerWalletCardsResponse>
{
    private readonly ICustomerWalletCardService _walletService;
    public GetCustomerWalletCardsQueryHandler(ICustomerWalletCardService walletService)
    {
        _walletService = walletService;
    }
    public async Task<GetCustomerWalletCardsResponse> Handle(GetCustomerWalletCardsQuery request, CancellationToken cancellationToken)
    {
        return await _walletService.GetCustomerWalletCardsAsync(request);
    }
}
