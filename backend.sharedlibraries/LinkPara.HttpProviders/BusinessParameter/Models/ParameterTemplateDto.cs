using LinkPara.HttpProviders.BusinessParameter.Models.Enums;

namespace LinkPara.HttpProviders.BusinessParameter.Models;

public class ParameterTemplateDto
{
    public Guid Id { get; set; }
    public string GroupCode { get; set; }
    public string TemplateCode { get; set; }
    public ParameterDataType DataType { get; set; }
    public int DataLength { get; set; }
    public string Explanation { get; set; }
}
