using LinkPara.SharedModels.Persistence;

namespace LinkPara.Kkb.Domain.Entities;
public class KkbAuthorizationToken : AuditEntity
{
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
    public string TokenType { get; set; }
    public DateTime ExpiresDate { get; set; }
    public bool IsSuccess { get; set; }
    public string Error { get; set; }
    public string ErrorDescription { get; set; }
}