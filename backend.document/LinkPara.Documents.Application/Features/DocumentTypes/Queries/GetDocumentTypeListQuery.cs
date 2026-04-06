using AutoMapper;
using AutoMapper.QueryableExtensions;
using LinkPara.Documents.Domain.Entities;
using LinkPara.MappingExtensions.Mapping;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using MediatR;

namespace LinkPara.Documents.Application.Features.DocumentTypes.Queries;

public class GetDocumentTypeListQuery : SearchQueryParams, IRequest<PaginatedList<DocumentTypeDto>>
{
}

public class GetDocumentTypeListQueryHandler : IRequestHandler<GetDocumentTypeListQuery, PaginatedList<DocumentTypeDto>>
{
    private readonly IGenericRepository<DocumentType> _documentTypesRepository;
    private readonly IMapper _mapper;

    public GetDocumentTypeListQueryHandler(IGenericRepository<DocumentType> documentTypesRepository,
        IMapper mapper)
    {
        _documentTypesRepository = documentTypesRepository;
        _mapper = mapper;

    }

    public async Task<PaginatedList<DocumentTypeDto>> Handle(GetDocumentTypeListQuery request, CancellationToken cancellationToken)
    {
        var query = _documentTypesRepository.GetAll();

        if (!string.IsNullOrEmpty(request.Q))
        {
            query = query.Where(x => x.Name.Contains(request.Q));
        }

        return await query
            .PaginatedListWithMappingAsync<DocumentType,DocumentTypeDto>(_mapper, request.Page, request.Size, request.OrderBy, request.SortBy);
    }
}
