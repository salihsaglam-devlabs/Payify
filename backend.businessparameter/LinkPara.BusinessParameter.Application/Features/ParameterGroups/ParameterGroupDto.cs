using LinkPara.BusinessParameter.Application.Commons.Mappings;
using LinkPara.BusinessParameter.Domain.Entities;
using LinkPara.BusinessParameter.Domain.Enums;

namespace LinkPara.BusinessParameter.Application.Features.ParameterGroups;

public class ParameterGroupDto : IMapFrom<ParameterGroup>
{
    public Guid Id { get; set; }
    public string GroupCode { get; set; }
    public string Explanation { get; set; }
    public string RecordStatus { get; set; }
    public ParameterDataType ParameterValueType { get; set; }
}
