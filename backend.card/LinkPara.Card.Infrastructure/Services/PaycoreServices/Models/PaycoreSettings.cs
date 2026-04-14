namespace LinkPara.Card.Infrastructure.Services.PaycoreServices.Models;

public class PaycoreSettings
{
    public string Token { get; set; }
    public string CreateCard { get; set; }
    public string GetCardInformation { get; set; }
    public string UpdateCardStatus { get; set; }
    public string GetCardAuthorization { get; set; }
    public string UpdateCardAuthorization { get; set; }
    public string GetCardEncryptedCvv2AndExpireDate { get; set; }
    public string SetCardPin { get; set; }
    public PaycoreVaultSettings VaultSettings { get; set; }
    public string CreateCustomer { get; set; }
    public string GetCustomer { get; set; }
    public string GetCustomerCards { get; set; }
    public string GetCustomerLimit { get; set; }
    public string UpdateCustomer { get; set; }
    public string UpdateCommunication { get; set; }
    public string UpdateAddress { get; set; }
    public string UpdateLimit { get; set; }
    public string GetCardLastCourierActivity { get; set; }
    public string AddAdditionalLimitRestriction { get; set; }
    public string CardRenewal { get; set; }
    public string GetClearCardNo { get; set; }
    public string GetProducts { get; set; }
    public string GetCardStatus { get; set; }
    public string ApiVersion { get; set; }
    public string Language { get; set; }
    public string Channel { get; set; }
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
    public string ClearZpkHex { get; set; }
}