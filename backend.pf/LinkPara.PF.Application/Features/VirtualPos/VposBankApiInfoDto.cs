using LinkPara.PF.Application.Commons.Mappings;
using LinkPara.PF.Application.Features.Banks;
using LinkPara.PF.Domain.Entities;

namespace LinkPara.PF.Application.Features.VirtualPos;

public class VposBankApiInfoDto : IMapFrom<VposBankApiInfo>
{
    public BankApiKeyDto Key { get; set; }
    public string Value { get; set; }
}


