using LinkPara.PF.Application.Commons.Mappings;
using LinkPara.PF.Domain.Entities;

namespace LinkPara.PF.Application.Commons.Models.Merchants;

public class UpdateMerchantUserRequest : IMapFrom<Domain.Entities.MerchantUser>
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Surname { get; set; }
    public DateTime BirthDate { get; set; }
    public string Email { get; set; }
    public string MobilePhoneNumber { get; set; }
    public string RoleId { get; set; }
    public string RoleName { get; set; }
    public Guid UserId { get; set; }
    public Guid MerchantId { get; set; }
}
