namespace LinkPara.ApiGateway.Merchant.Services.Identity.Models.Requests;

public class CreateUserSecurityPictureRequest
{
    public Guid UserId { get; set; }
    public Guid SecurityPictureId { get; set; }
}