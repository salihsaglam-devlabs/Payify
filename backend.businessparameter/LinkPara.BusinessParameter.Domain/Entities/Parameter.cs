using LinkPara.SharedModels.Persistence;

namespace LinkPara.BusinessParameter.Domain.Entities;

public class Parameter : AuditEntity, ITrackChange
{
    public string GroupCode { get; set; }
    public string ParameterCode { get; set; }
    public string ParameterValue { get; set; }
    public ParameterGroup ParameterGroup { get; set; }
}