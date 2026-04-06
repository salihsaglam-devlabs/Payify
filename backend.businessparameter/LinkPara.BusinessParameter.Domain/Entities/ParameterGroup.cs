using LinkPara.BusinessParameter.Domain.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.BusinessParameter.Domain.Entities;

public class ParameterGroup : AuditEntity, ITrackChange
{
    public string GroupCode { get; set; }
    public string Explanation { get; set; }
    public ParameterDataType ParameterValueType { get; set; }
}