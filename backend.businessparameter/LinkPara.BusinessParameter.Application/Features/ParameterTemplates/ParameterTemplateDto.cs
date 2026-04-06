using LinkPara.BusinessParameter.Application.Commons.Mappings;
using LinkPara.BusinessParameter.Domain.Entities;
using LinkPara.BusinessParameter.Domain.Enums;

namespace LinkPara.BusinessParameter.Application.Features.ParameterTemplates;

public class ParameterTemplateDto : IMapFrom<ParameterTemplate>
{
    public Guid Id { get; set; }
    public string GroupCode { get; set; }
    public string TemplateCode { get; set; }
    public ParameterDataType DataType { get; set; }
    public int DataLength { get; set; }
    public string Explanation { get; set; }
}
