using LinkPara.Approval.Domain.Enums;
using LinkPara.Approval.Models.Enums;
using LinkPara.SharedModels.Persistence;
using System.ComponentModel.DataAnnotations.Schema;

namespace LinkPara.Approval.Domain.Entities;

public class Case : AuditEntity, ITrackChange
{
    public string BaseUrl { get; set; }
    public string ActionName { get; set; }
    public CaseType CaseType { get; set; }
    public string Resource { get; set; }
    public ActionType Action { get; set; }
    public string ModuleName { get; set; }
    public string DisplayName { get; set; }
    public virtual List<MakerChecker> MakerCheckers { get; set; }
}
