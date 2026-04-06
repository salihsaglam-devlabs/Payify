using LinkPara.PF.Application.Commons.Mappings;
using LinkPara.PF.Application.Features.SubMerchants;
using LinkPara.PF.Domain.Entities;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.PF.Application.Features.SubMerchantUsers;

public class SubMerchantUserDto : IMapFrom<SubMerchantUser>
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Surname { get; set; }
    public DateTime BirthDate { get; set; }
    public string Email { get; set; }
    public string MobilePhoneNumber { get; set; }
    public string PhoneCode { get; set; }
    public string RoleId { get; set; }
    public string RoleName { get; set; }
    public string IdentityNumber { get; set; }
    public Guid UserId { get; set; }
    public Guid SubMerchantId { get; set; }
    public string CreatedBy { get; set; }
    public DateTime CreateDate { get; set; }
    public RecordStatus RecordStatus { get; set; }
    public SubMerchantDto SubMerchant { get; set; }
}
