using LinkPara.PF.Application.Commons.Mappings;
using LinkPara.PF.Domain.Entities;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.PF.Application.Commons.Models.SubMerchants;

public class SubMerchantUserModel : IMapFrom<SubMerchantUser>
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Surname { get; set; }
    public RecordStatus RecordStatus { get; set; }
}
