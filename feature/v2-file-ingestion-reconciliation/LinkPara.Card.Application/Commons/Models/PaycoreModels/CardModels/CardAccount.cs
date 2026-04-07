namespace LinkPara.Card.Application.Commons.Models.PaycoreModels.CardModels;
public class CardAccount
{
    public AccountCommunication CrdAccountCommunication { get; set; }
}
public class AccountCommunication
{
    public string MobilePhone { get; set; }
    public string Email { get; set; }
}
public class CardDelivery
{
    public PaycoreAddress CardPostAddress { get; set; }
}