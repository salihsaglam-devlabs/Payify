using LinkPara.Identity.Domain.Entities;

namespace LinkPara.Identity.Application.Common.Interfaces
{
    public interface IAgreementDocumentService
    {
        Task UpdateAgreementDocument(AgreementDocument entity);
    }
}
