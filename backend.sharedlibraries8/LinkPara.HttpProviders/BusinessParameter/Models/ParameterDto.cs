using LinkPara.SharedModels.Persistence;

namespace LinkPara.HttpProviders.BusinessParameter.Models;

public class ParameterDto
{
    public Guid Id { get; set; }
    public string GroupCode { get; set; }
    public string ParameterCode { get; set; }
    public string ParameterValue { get; set; }
    public RecordStatus RecordStatus { get; set; }
}
