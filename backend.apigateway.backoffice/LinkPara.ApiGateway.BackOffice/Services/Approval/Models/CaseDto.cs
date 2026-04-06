using LinkPara.ApiGateway.BackOffice.Services.Approval.Models.Enums;
using LinkPara.Approval.Models.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.BackOffice.Services.Approval.Models;

public class CaseDto
{
    public Guid Id { get; set; }
    public string BaseUrl { get; set; }
    public string Resource { get; set; }
    public ActionType Action { get; set; }
    public string DisplayName { get; set; }
    public string ActionName { get; set; }
    public CaseType CaseType { get; set; }
    public List<MakerCheckerDto> MakerCheckers { get; set; }
    public RecordStatus RecordStatus { get; set; }
    public string ModuleName { get; set; }
}
