namespace LinkPara.Card.Application.Commons.Models.WalletMoodels.CardModels;

public class GetCustomerWalletCardsResponse
{
    public List<CustomerWalletCardsResponse> CustomerWalletCards {  get; set; }
    public bool IsSuccess { get; set; }
    public string Description { get; set; }
}

public class CustomerWalletCardsResponse
{
    public string BankingCustomerNo { get; set; }
    public string WalletNumber { get; set; }
    public string CardNumber { get; set; }
    public string ProductCode { get; set; }
    public string CardStatus { get; set; }
    public string CardName{ get; set; }
    public bool IsActive { get; set; }
}