namespace LinkPara.ApiGateway.Services.Identity.Models.Responses;

public class UserPictureDto
{
    public byte[] Bytes { get; set; }
    public string ContentType { get; set; }
    public string UserId { get; set; }
}
