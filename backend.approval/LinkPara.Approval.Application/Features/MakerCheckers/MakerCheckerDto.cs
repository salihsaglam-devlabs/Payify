using LinkPara.Approval.Application.Commons.Mappings;
using LinkPara.Approval.Domain.Entities;

namespace LinkPara.Approval.Application.Features.MakerCheckers;

public class MakerCheckerDto : IMapFrom<MakerChecker>
{
    public Guid Id { get; set; }
    public Guid CaseId { get; set; }
    public Guid MakerRoleId { get; set; }
    public Guid CheckerRoleId { get; set; }
    public Guid SecondCheckerRoleId { get; set; }
}
