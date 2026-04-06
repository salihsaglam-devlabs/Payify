using System.ComponentModel.DataAnnotations.Schema;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.Identity.Domain.Entities;

public class UserAgreementDocument : AuditEntity, ITrackChange
{
    public Guid UserId { get; set; }
    public User User { get; set; }
    public Guid AgreementDocumentVersionId { get; set; }
    public AgreementDocumentVersion AgreementDocumentVersion { get; set; }
    public string ApprovalChannel { get; set; }
}