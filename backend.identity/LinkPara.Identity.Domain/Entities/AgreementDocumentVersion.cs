using LinkPara.SharedModels.Persistence;

namespace LinkPara.Identity.Domain.Entities;

public class AgreementDocumentVersion : AuditEntity, ITrackChange
{
    public Guid AgreementDocumentId { get; set; }
    public AgreementDocument AgreementDocument { get; set; }
    public string Content { get; set; }
    public string Title { get; set; }
    public string LanguageCode { get; set; }
    public string Version { get; set; }
    public bool IsLatest { get; set; }
    public bool IsForceUpdate { get; set; }
    public bool IsOptional { get; set; }
}