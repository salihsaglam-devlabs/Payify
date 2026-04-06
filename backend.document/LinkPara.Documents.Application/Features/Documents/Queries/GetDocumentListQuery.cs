using AutoMapper;
using AutoMapper.QueryableExtensions;
using LinkPara.Documents.Domain.Entities;
using LinkPara.MappingExtensions.Mapping;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LinkPara.Documents.Application.Features.Documents.Queries;

public class GetDocumentListQuery : SearchQueryParams, IRequest<PaginatedList<DocumentResponse>>
{
    public Guid? UserId { get; set; }
    public Guid? MerchantId { get; set; }
    public Guid? SubMerchantId { get; set; }
    public Guid? DocumentTypeId { get; set; }
    public bool OnlyLatest { get; set; }
    public Guid? AccountId { get; set; }
}

public class GetDocumentListQueryHandler : IRequestHandler<GetDocumentListQuery, PaginatedList<DocumentResponse>>
{
    private readonly IGenericRepository<Document> _documentsRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<Document> _logger;

    public GetDocumentListQueryHandler(IGenericRepository<Document> documentsRepository,
        IMapper mapper, ILogger<Document> logger)
    {
        _documentsRepository = documentsRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public ILogger<Document> Logger { get; }

    public async Task<PaginatedList<DocumentResponse>> Handle(GetDocumentListQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var query = _documentsRepository.GetAll()
           .Where(x => x.RecordStatus == RecordStatus.Active);

            if (request.UserId.HasValue)
            {
                query = query.Where(x => x.UserId == request.UserId);
            }
            if (request.MerchantId.HasValue)
            {
                query = query.Where(x => x.MerchantId == request.MerchantId);
            }
            if (request.SubMerchantId.HasValue)
            {
                query = query.Where(x => x.SubMerchantId == request.SubMerchantId);
            }
            if (request.DocumentTypeId.HasValue)
            {
                query = query.Where(x => x.DocumentTypeId == request.DocumentTypeId);
            }
            if (request.AccountId.HasValue)
            {
                query = query.Where(x => x.AccountId == request.AccountId);
            }

            if (request.OnlyLatest)
            {
                query = query.OrderByDescending(x => x.Id);

                var result = await query.GroupBy(x => x.DocumentTypeId)
                    .Select(x => x.FirstOrDefault())
                    .ToListAsync();

                return new()
                {
                    Items = _mapper.Map<List<DocumentResponse>>(result),
                    TotalCount = result.Count
                };
            }

            return await query
                .PaginatedListWithMappingAsync<Document, DocumentResponse>(_mapper, request.Page, request.Size, request.OrderBy, request.SortBy);
        }
        catch (Exception exception)
        {
            _logger.LogError($"GetDocumentsError : {exception}");
            throw;
        }       
    }
}
