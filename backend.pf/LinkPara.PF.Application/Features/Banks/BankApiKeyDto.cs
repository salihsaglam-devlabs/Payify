using LinkPara.PF.Application.Commons.Mappings;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.PF.Application.Features.Banks;

public class BankApiKeyDto : IMapFrom<BankApiKey>
{
    public Guid Id { get; set; }
    public string Key { get; set; }
    public string MappingName { get; set; }
    public BankApiKeyCategory Category { get; set; }
    public RecordStatus RecordStatus { get; set; }
}
