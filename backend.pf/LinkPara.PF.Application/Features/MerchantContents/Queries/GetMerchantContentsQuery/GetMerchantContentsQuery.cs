using AutoMapper;
using LinkPara.MappingExtensions.Mapping;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.PF.Application.Features.MerchantContents.Queries.GetMerchantContentsQuery;

public class GetMerchantContentsQuery : SearchQueryParams, IRequest<PaginatedList<MerchantContentDto>>
{
    public Guid? MerchantId { get; set; }
    public MerchantContentSource? ContentSource { get; set; }
    public string Name { get; set; }
}

public class GetMerchantContentsQueryHandler : IRequestHandler<GetMerchantContentsQuery, PaginatedList<MerchantContentDto>>
{
    private readonly IGenericRepository<MerchantContent> _repository;
    private readonly IMapper _mapper;

    public GetMerchantContentsQueryHandler(
        IGenericRepository<MerchantContent> repository,
        IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<PaginatedList<MerchantContentDto>> Handle(GetMerchantContentsQuery request, CancellationToken cancellationToken)
    {
        var query = _repository
            .GetAll()
            .Include(lc => lc.Contents.Where(a => a.RecordStatus == RecordStatus.Active))
            .Where(lc => lc.RecordStatus == RecordStatus.Active);

        if (request.MerchantId.HasValue)
        {
            query = query.Where(lc => lc.MerchantId == request.MerchantId);
        }
        
        if (request.ContentSource.HasValue)
        {
            query = query.Where(lc => lc.ContentSource == request.ContentSource);
        }

        if (!string.IsNullOrEmpty(request.Name))
        {
            query = query.Where(lc => lc.Name.ToLower()
                .Contains(request.Name.ToLower()));
        }
        
        return await query
            .PaginatedListWithMappingAsync<MerchantContent,MerchantContentDto>(_mapper, request.Page, request.Size, request.OrderBy, request.SortBy);
    }
}