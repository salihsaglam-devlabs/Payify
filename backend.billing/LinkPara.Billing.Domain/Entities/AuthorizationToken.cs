using LinkPara.SharedModels.Persistence;
using System.ComponentModel.DataAnnotations.Schema;

namespace LinkPara.Billing.Domain.Entities;

public class AuthorizationToken : AuditEntity
{
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
    public string TokenType { get; set; }
    public Guid VendorId { get; set; }
    public DateTime ExpiryDate { get; set; }
    public DateTime RegisterDate { get; set; }
}