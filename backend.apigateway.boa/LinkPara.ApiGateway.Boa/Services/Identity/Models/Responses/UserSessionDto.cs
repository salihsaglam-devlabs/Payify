using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.Boa.Services.Identity.Models.Responses;

public class UserSessionDto
{
    public Guid UserId { get; set; }
    public string RefreshToken { get; set; }
    public DateTime RefreshTokenExpiration { get; set; }
    public RecordStatus RecordStatus { get; set; }
}
