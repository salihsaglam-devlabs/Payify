using LinkPara.PF.Application.Commons.Mappings;
using LinkPara.PF.Domain.Entities;

namespace LinkPara.PF.Application.Features.VirtualPos;

public class SaveBankApiInfoDto : IMapFrom<VposBankApiInfo>
{
    public Guid KeyId { get; set; }
    public string Value { get; set; }
}
