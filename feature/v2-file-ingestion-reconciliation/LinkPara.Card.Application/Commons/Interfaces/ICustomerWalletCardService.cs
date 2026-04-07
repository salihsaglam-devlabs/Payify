using LinkPara.Card.Application.Commons.Models.WalletMoodels.CardModels;
using LinkPara.Card.Application.Features.WalletServices.CardServices.Queries.GetCustomerWalletCardsQuery;

namespace LinkPara.Card.Application.Commons.Interfaces;

public interface ICustomerWalletCardService
{
    Task<GetCustomerWalletCardsResponse> GetCustomerWalletCardsAsync(GetCustomerWalletCardsQuery command);
}

