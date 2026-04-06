using LinkPara.BusinessParameter.Application.Commons.Mappings;
using LinkPara.BusinessParameter.Application.Features.ParameterTemplateValues;
using LinkPara.BusinessParameter.Domain.Entities;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.BusinessParameter.Application.Features.Parameters;

public class ParameterDto : IMapFrom<Parameter>
{
    public Guid Id { get; set; }
    public string GroupCode { get; set; }
    public string ParameterCode { get; set; }
    public string ParameterValue { get; set; }
    public List<ParameterTemplateValueResponse> ParameterTemplateValueList { get; set; }
    public RecordStatus RecordStatus { get; set; }
}
