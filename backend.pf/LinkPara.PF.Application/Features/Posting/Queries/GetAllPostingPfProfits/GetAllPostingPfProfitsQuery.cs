using AutoMapper;
using LinkPara.MappingExtensions.Mapping;
using LinkPara.PF.Domain.Entities;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.PF.Application.Features.Posting.Queries.GetAllPostingPfProfits;

public class GetAllPostingPfProfitsQuery : SearchQueryParams, IRequest<PaginatedList<PostingPfProfitDto>>
{
    public DateTime? PaymentDate { get; set; }
}

public class GetAllPostingPfProfitsQueryHandler 
    : IRequestHandler<GetAllPostingPfProfitsQuery, PaginatedList<PostingPfProfitDto>>
{
    private readonly IGenericRepository<PostingPfProfit> _repository;
    private readonly IMapper _mapper;

    public GetAllPostingPfProfitsQueryHandler(
        IGenericRepository<PostingPfProfit> repository, 
        IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<PaginatedList<PostingPfProfitDto>> Handle(GetAllPostingPfProfitsQuery request, CancellationToken cancellationToken)
    {
        var postingPfProfits = _repository.GetAll()
            .Include(r => r.PostingPfProfitDetails)
            .AsQueryable();

        if (request.PaymentDate is not null)
        {
            postingPfProfits = postingPfProfits.Where(p => p.PaymentDate == request.PaymentDate);
        }
        
        return await postingPfProfits
            .PaginatedListWithMappingAsync<PostingPfProfit,PostingPfProfitDto>(_mapper, request.Page, request.Size, request.OrderBy, request.SortBy);
    }
}