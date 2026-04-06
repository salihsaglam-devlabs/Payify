namespace LinkPara.ApiGateway.Services.Emoney.Models.Requests;

public class UpdateAccountRequest
{
    public bool IsOpenBankingPermit { get; set; }
    public string Email { get; set; }
    public string Profession { get; set; }
    public string PhoneCode { get; set; }
    public string PhoneNumber { get; set; }
}
