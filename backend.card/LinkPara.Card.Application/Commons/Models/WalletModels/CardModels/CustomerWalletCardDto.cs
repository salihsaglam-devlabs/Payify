using LinkPara.Card.Application.Commons.Mappings;
using LinkPara.Card.Domain.Entities;

namespace LinkPara.Card.Application.Commons.Models.WalletModels.CardModels;
public class CustomerWalletCardDto : IMapFrom<CustomerWalletCard>
{
    public string BankingCustomerNo { get; set; }
    public string CardNumber { get; set; }
    public string ProductCode { get; set; }
    public string CardStatus { get; set; }
    public string CardName { get; set; }
}
