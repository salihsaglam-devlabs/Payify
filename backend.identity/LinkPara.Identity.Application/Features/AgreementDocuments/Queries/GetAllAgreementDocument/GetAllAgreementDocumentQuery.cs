using AutoMapper;
using AutoMapper.QueryableExtensions;
using LinkPara.Identity.Application.Common.Interfaces;
using LinkPara.Identity.Domain.Entities;
using LinkPara.Identity.Domain.Enums;
using LinkPara.MappingExtensions.Mapping;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace LinkPara.Identity.Application.Features.AgreementDocuments.Queries.GetAllAgreementDocument;
public class GetAllAgreementDocumentQuery : SearchQueryParams, IRequest<PaginatedList<AgreementDocumentResponse>>
{
    public RecordStatus? RecordStatus { get; set; }
    public string AgreementTitle { get; set; }
    public ProductType? ProductType { get; set; }

}

public class GetAllAgreementDocumentQueryHandler : IRequestHandler<GetAllAgreementDocumentQuery, PaginatedList<AgreementDocumentResponse>>
{
    private readonly IRepository<AgreementDocument> _repository;
    private readonly IMapper _mapper;

    public GetAllAgreementDocumentQueryHandler(IRepository<AgreementDocument> repository,
       IMapper mapper,
       IRepository<AgreementDocumentVersion> agreementDocumentVersion,
       IHttpContextAccessor httpContextAccessor)
    {
        _repository = repository;
        _mapper = mapper;
    }
    public async Task<PaginatedList<AgreementDocumentResponse>> Handle(GetAllAgreementDocumentQuery request, CancellationToken cancellationToken)
    {
        var documentList = _repository.GetAll();

        if (request.RecordStatus.HasValue)
        {
            documentList = documentList.Where(x => x.RecordStatus == request.RecordStatus);
        }

        if (!String.IsNullOrEmpty(request.AgreementTitle))
        {
            documentList = documentList.Where(x => x.Name.ToLower()
                                       .Contains(request.AgreementTitle.ToLower()));
        }

        if (request.ProductType.HasValue)
        {
            documentList = documentList.Where(x => x.ProductType == request.ProductType);
        }

        return await documentList
            .PaginatedListWithMappingAsync<AgreementDocument,AgreementDocumentResponse>(_mapper, request.Page, request.Size, request.OrderBy, request.SortBy);
    }
}
