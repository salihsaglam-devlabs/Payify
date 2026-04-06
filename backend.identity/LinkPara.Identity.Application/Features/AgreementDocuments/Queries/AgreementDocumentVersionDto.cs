
using LinkPara.Identity.Application.Common.Mappings;
using LinkPara.Identity.Domain.Entities;

namespace LinkPara.Identity.Application.Features.AgreementDocuments.Queries;

public class AgreementDocumentVersionDto : IMapFrom<AgreementDocumentVersion>
{
    public Guid AgreementDocumentId { get; set; }
    public string Version { get; set; }
    public string Content { get; set; }
    public string Title { get; set; }
    public bool IsForceUpdate { get; set; }
    public bool IsOptional { get; set; }
}
