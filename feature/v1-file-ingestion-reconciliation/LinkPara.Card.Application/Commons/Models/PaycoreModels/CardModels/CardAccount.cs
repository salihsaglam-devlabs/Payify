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
    public CardPostAddress CardPostAddress { get; set; }
}
public class CardPostAddress
{
    public string AddressType { get; set; }
    public string TownCode { get; set; }
    public string CityCode { get; set; }
    public string CountryCode { get; set; }
    public string Address1 { get; set; }
    public string Address2 { get; set; }
    public string Address3 { get; set; }
    public object Address4 { get; set; }
    public string District { get; set; }
    public object ZipCode { get; set; }
}