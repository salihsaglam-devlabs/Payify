namespace LinkPara.ApiGateway.Merchant.Services.Pf.Models.Responses;

public class MerchantLogoDto
{
    public Guid MerchantId { get; set; }
    public byte[] Bytes { get; set; }
    public string ContentType { get; set; }
    public string FileName { get; set; }
}