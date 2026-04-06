using LinkPara.HttpProviders.BusinessParameter.Models.Enums;

namespace LinkPara.HttpProviders.BusinessParameter.Models;

public class ParameterGroupDto
{
    public Guid Id { get; set; }
    public string GroupCode { get; set; }
    public string Explanation { get; set; }
    public ParameterDataType ParameterValueType { get; set; }
}
