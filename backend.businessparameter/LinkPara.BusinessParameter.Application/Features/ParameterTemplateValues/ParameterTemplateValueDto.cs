using LinkPara.BusinessParameter.Application.Commons.Mappings;
using LinkPara.BusinessParameter.Domain.Entities;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.BusinessParameter.Application.Features.ParameterTemplateValues;

public class ParameterTemplateValueDto : IMapFrom<ParameterTemplateValue>
{
    public Guid Id { get; set; }
    public string GroupCode { get; set; }
    public string ParameterCode { get; set; }
    public string ParameterValue { get; set; }
    public string TemplateCode { get; set; }
    public string TemplateValue { get; set; }
    public RecordStatus RecordStatus { get; set; }
}
