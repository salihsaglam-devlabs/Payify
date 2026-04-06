using LinkPara.SharedModels.Persistence;

namespace LinkPara.CampaignManagement.Domain.Entities;
public class AuthorizationToken : AuditEntity
{
    public string Token { get; set; }
    public DateTime ExpiryDate { get; set; }
    public DateTime RefreshTokenDate { get; set; }
}
