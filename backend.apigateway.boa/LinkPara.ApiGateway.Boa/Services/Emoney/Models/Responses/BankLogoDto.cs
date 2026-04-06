namespace LinkPara.ApiGateway.Boa.Services.Emoney.Models.Responses;

public class BankLogoDto
{
    public byte[] Bytes { get; set; }
    public string ContentType { get; set; }
    public Guid BankLogo { get; set; }
}