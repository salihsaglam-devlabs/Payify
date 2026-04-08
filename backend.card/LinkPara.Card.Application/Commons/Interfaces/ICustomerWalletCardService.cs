using LinkPara.Card.Application.Commons.Models.WalletModels.CardModels;
using LinkPara.Card.Application.Features.WalletServices.CardServices.Queries.GetCustomerWalletCardsQuery;

namespace LinkPara.Card.Application.Commons.Interfaces;

public interface ICustomerWalletCardService
{
    Task<List<CustomerWalletCardDto>> GetCustomerWalletCardsAsync(GetCustomerWalletCardsQuery command);
}

