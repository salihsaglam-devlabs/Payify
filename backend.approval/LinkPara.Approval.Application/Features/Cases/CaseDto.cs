using LinkPara.Approval.Application.Commons.Mappings;
using LinkPara.Approval.Application.Features.MakerCheckers;
using LinkPara.Approval.Domain.Entities;
using LinkPara.Approval.Domain.Enums;
using LinkPara.Approval.Models.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.Approval.Application.Features.Cases;

public class CaseDto : IMapFrom<Case>
{
    public Guid Id { get; set; }
    public string BaseUrl { get; set; }
    public string ActionName { get; set; }
    public CaseType CaseType { get; set; }
    public string Resource { get; set; }
    public ActionType Action { get; set; }
    public string DisplayName { get; set; }
    public List<MakerCheckerDto> MakerCheckers { get; set; }
    public RecordStatus RecordStatus { get; set; }
    public string ModuleName { get; set; }
}
