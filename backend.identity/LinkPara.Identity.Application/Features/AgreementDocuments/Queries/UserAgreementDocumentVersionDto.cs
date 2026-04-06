using LinkPara.Identity.Application.Common.Mappings;
using LinkPara.Identity.Domain.Entities;

namespace LinkPara.Identity.Application.Features.AgreementDocuments.Queries;

public class UserAgreementDocumentVersionDto : IMapFrom<UserAgreementDocument>
{
    public string UserId { get; set; }
    public string AgreementDocumentName { get; set; }
    public string Version { get; set; }
    public bool IsForceUpdate { get; set; }
}
