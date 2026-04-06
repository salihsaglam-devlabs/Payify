using LinkPara.SharedModels.Persistence;

namespace LinkPara.HttpProviders.BusinessParameter.Models;

public class ParameterTemplateValueDto
{
    public Guid Id { get; set; }
    public string GroupCode { get; set; }
    public string ParameterCode { get; set; }
    public string TemplateCode { get; set; }
    public string TemplateValue { get; set; }
    public RecordStatus RecordStatus { get; set; }

}
