using LinkPara.Approval.Application.Commons.Mappings;
using LinkPara.Approval.Domain.Entities;
using LinkPara.Approval.Domain.Enums;

namespace LinkPara.Approval.Application.Features.Requests;

public class PatchRequestDto : IMapFrom<Request>
{
    public ApprovalStatus Status { get; set; }
    public string ExceptionMessage { get; set; }
    public string ExceptionDetails { get; set; }
}
