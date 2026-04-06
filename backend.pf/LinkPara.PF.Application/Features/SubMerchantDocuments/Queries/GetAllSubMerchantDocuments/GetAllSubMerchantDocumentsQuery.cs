using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Models.SubMerchants;
using LinkPara.SharedModels.Pagination;
using MediatR;

namespace LinkPara.PF.Application.Features.SubMerchantDocuments.Queries.GetAllSubMerchantDocuments;

public class GetAllSubMerchantDocumentsQuery : SearchQueryParams, IRequest<PaginatedList<SubMerchantDocumentDto>>
{
    public Guid? DocumentId { get; set; }
    public string DocumentName { get; set; }
    public Guid? DocumentTypeId { get; set; }
    public Guid? SubMerchantId { get; set; }
}

public class GetAllSubMerchantDocumentsQueryHandler : IRequestHandler<GetAllSubMerchantDocumentsQuery, PaginatedList<SubMerchantDocumentDto>>
{
    private readonly ISubMerchantDocumentService _subMerchantDocumentService;

    public GetAllSubMerchantDocumentsQueryHandler(ISubMerchantDocumentService subMerchantDocumentService)
    {
        _subMerchantDocumentService = subMerchantDocumentService;
    }

    public async Task<PaginatedList<SubMerchantDocumentDto>> Handle(GetAllSubMerchantDocumentsQuery request, CancellationToken cancellationToken)
    {
        return await _subMerchantDocumentService.GetListAsync(request);
    }
}