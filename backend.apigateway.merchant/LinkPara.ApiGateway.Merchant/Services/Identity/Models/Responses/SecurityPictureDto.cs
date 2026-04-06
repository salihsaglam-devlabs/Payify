namespace LinkPara.ApiGateway.Merchant.Services.Identity.Models.Responses;

public class SecurityPictureDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public byte[] Bytes { get; set; }
    public string ContentType { get; set; }
}
