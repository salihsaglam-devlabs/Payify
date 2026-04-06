using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Models.SubMerchants;
using MediatR;

namespace LinkPara.PF.Application.Features.SubMerchantDocuments.Queries.GetSubMerchantDocumentById;

public class GetSubMerchantDocumentByIdQuery : IRequest<SubMerchantDocumentDto>
{
    public Guid DocumentId { get; set; }
}

public class GetSubMerchantDocumentByIdQueryHandler : IRequestHandler<GetSubMerchantDocumentByIdQuery, SubMerchantDocumentDto>
{
    private readonly ISubMerchantDocumentService _subMerchantDocumentService;

    public GetSubMerchantDocumentByIdQueryHandler(ISubMerchantDocumentService subMerchantDocumentService)
    {
        _subMerchantDocumentService = subMerchantDocumentService;
    }

    public async Task<SubMerchantDocumentDto> Handle(GetSubMerchantDocumentByIdQuery request, CancellationToken cancellationToken)
    {
        return await _subMerchantDocumentService.GetByIdAsync(request.DocumentId);
    }
}
