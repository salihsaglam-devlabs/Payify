using LinkPara.SharedModels.Persistence;

namespace LinkPara.PF.Domain.Entities;

public class SubMerchantUser : AuditEntity
{
    public Guid UserId { get; set; }
    public string Name { get; set; }
    public string Surname { get; set; }
    public DateTime BirthDate { get; set; }
    public string Email { get; set; }
    public string MobilePhoneNumber { get; set; }
    public string RoleId { get; set; }
    public string RoleName { get; set; }
    public string IdentityNumber { get; set; }
    public Guid SubMerchantId { get; set; }
    public virtual SubMerchant SubMerchant { get; set; }
}