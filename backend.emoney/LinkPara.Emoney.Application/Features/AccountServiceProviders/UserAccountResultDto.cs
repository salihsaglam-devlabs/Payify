namespace LinkPara.Emoney.Application.Features.AccountServiceProviders;

public class UserAccountResultDto
{
    public string AccountName { get; set; }
    public string IbanNo { get; set; }
    public string Fec { get; set; }
    public string CustomerName { get; set; }
    public string AvailableBalance { get; set; }
}