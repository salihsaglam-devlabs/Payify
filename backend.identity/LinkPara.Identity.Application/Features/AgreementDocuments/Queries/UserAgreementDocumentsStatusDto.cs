
namespace LinkPara.Identity.Application.Features.AgreementDocuments.Queries;

public class UserAgreementDocumentsStatusDto
{
    public Guid AgreementDocumentId { get; set; }
    public string Title { get; set; }
    public string Version { get; set; }
    public bool IsLatest { get; set; }
    public bool IsForceUpdate { get; set; }
    public bool IsSigned { get; set; }
    public string Content { get; set; }
    public DateTime? SignedAt { get; set; }
    public string ApprovalChannel { get; set; }

}
