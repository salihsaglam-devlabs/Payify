using LinkPara.PF.Application.Commons.Models.SubMerchants;
using LinkPara.PF.Application.Features.SubMerchantDocuments.Commands.SaveSubMerchantDocument;
using LinkPara.PF.Application.Features.SubMerchantDocuments.Commands.UpdateSubMerchantDocument;
using LinkPara.PF.Application.Features.SubMerchantDocuments.Queries.GetAllSubMerchantDocuments;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.PF.Application.Commons.Interfaces;

public interface ISubMerchantDocumentService
{
    Task<PaginatedList<SubMerchantDocumentDto>> GetListAsync(GetAllSubMerchantDocumentsQuery request);
    Task<SubMerchantDocumentDto> GetByIdAsync(Guid documentId);
    Task DeleteAsync(Guid documentId);
    Task SaveAsync(SaveSubMerchantDocumentCommand request);
    Task UpdateAsync(UpdateSubMerchantDocumentCommand request);
}
