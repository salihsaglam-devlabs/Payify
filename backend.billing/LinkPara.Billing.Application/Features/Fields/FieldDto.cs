using LinkPara.Billing.Application.Commons.Mappings;
using LinkPara.Billing.Domain.Entities;

namespace LinkPara.Billing.Application.Features.Fields;

public class FieldDto : IMapFrom<Field>
{
    public string Label { get; set; }
    public string Mask { get; set; }
    public string Pattern { get; set; }
    public string Placeholder { get; set; }
    public int Length { get; set; }
    public int Order { get; set; }
    public string Prefix { get; set; }
    public string Suffix { get; set; }
}