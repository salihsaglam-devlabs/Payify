namespace LinkPara.ApiGateway.OpenBanking.Services.Emoney.Responses;

public class ApprovalScreenWalletDto
{
    public string CustomerName { get; set; }
    public string Iban {  get; set; }
    public string ReferenceNumber { get; set; }
    public string ProductName { get; set; }
    public string Currency {  get; set; }
}
