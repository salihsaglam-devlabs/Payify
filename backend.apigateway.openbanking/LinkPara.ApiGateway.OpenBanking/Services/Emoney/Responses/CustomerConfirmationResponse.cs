namespace LinkPara.ApiGateway.OpenBanking.Services.Emoney.Responses;

public class CustomerConfirmationResponse
{
    public bool IsCustomerValid { get; set; }
    public string CustomerId { get; set; }
    public string Unv { get; set; }
    public string HspNo { get; set; }
    public string KolasRefNo { get; set; }
    public string KolasHspTur { get; set; }
    public string OdmStm { get; set; }
    public string BekOdmZmn { get; set; }
    public bool HasMobileDevice { get; set; }
    public bool HasMultipleCustomerFound { get; set; }
}