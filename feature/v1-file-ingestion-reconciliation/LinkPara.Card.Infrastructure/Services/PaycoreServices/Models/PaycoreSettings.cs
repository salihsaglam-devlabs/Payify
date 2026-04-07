namespace LinkPara.Card.Infrastructure.Services.PaycoreServices.Models;

public class PaycoreSettings
{
    public string Token { get; set; }
    public string CreateCard { get; set; }
    public string GetCardInformation { get; set; }
    public string UpdateCardStatus { get; set; }
    public string GetCardAuthorization { get; set; }
    public PaycoreVaultSettings VaultSettings { get; set; }
    public string CreateCustomer { get; set; }
    public string GetCustomer { get; set; }
    public string GetCustomerCards { get; set; }
    public string GetCustomerLimit { get; set; }
    public string UpdateCustomer { get; set; }
    public string UpdateCommunication { get; set; }
    public string UpdateAddress { get; set; }
    public string UpdateLimit { get; set; }

}

public class PaycoreVaultSettings
{
    public int MbrId { get; set; }
    public string UserCode { get; set; }
    public string Password { get; set; }
    public int SessionTimeout { get; set; }
    public string BaseUrl { get; set; }
    public int BranchCode { get; set; }
    public string LanguageCode { get; set; }
    public string CustomerGroupCode { get; set; }
    public string ProductCode { get; set; }
    public string CardLevel { get; set; }//"M" olacak
}