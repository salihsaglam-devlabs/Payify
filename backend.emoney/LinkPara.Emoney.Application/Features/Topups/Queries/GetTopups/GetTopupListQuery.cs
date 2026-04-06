using AutoMapper;
using LinkPara.Emoney.Application.Commons.Enums;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.Emoney.Domain.Enums;
using LinkPara.MappingExtensions.Mapping;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using MediatR;

namespace LinkPara.Emoney.Application.Features.Topups.Queries.GetTopups;

public class GetTopupListQuery : SearchQueryParams, IRequest<PaginatedList<TopupDto>>
{
    public string Name { get; set; }
    public CardTopupRequestStatus? Status { get; set; }
    public string WalletNumber { get; set; }
    public string BinNumber { get; set; }
    public RecordStatus? RecordStatus { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public PaymentProviderType? PaymentProviderType { get; set; }
    public CardType? CardType { get; set; }
}

public class GetTopupsQueryHandler : IRequestHandler<GetTopupListQuery, PaginatedList<TopupDto>>
{
    private readonly IGenericRepository<CardTopupRequest> _repository;
    private readonly IMapper _mapper;

    public GetTopupsQueryHandler(IGenericRepository<CardTopupRequest> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<PaginatedList<TopupDto>> Handle(GetTopupListQuery request, CancellationToken cancellationToken)
    {
        var query = _repository
            .GetAll();

        query = ApplyStringFilter(request, query);
        query = ApplyStatusFilter(request, query);
        query = ApplyDateFilter(request, query);
        query = ApplyEnumFilter(request, query);

        return await query.PaginatedListWithMappingAsync<CardTopupRequest, TopupDto>(_mapper, request.Page, request.Size, request.OrderBy, request.SortBy);
    }

    private static IQueryable<CardTopupRequest> ApplyDateFilter(GetTopupListQuery request, IQueryable<CardTopupRequest> query)
    {
        if (request.StartDate.HasValue)
        {
            query = query.Where(x => x.CreateDate >= request.StartDate);
        }
        if (request.EndDate.HasValue)
        {
            query = query.Where(x => x.CreateDate <= request.EndDate);
        }
        return query;
    }

    private static IQueryable<CardTopupRequest> ApplyStatusFilter(GetTopupListQuery request, IQueryable<CardTopupRequest> query)
    {
        if (request.RecordStatus.HasValue)
        {
            query = query.Where(t => t.RecordStatus == request.RecordStatus);
        }
        if (request.Status.HasValue)
        {
            query = query.Where(t => t.Status == request.Status);
        }
        return query;
    }

    private static IQueryable<CardTopupRequest> ApplyStringFilter(GetTopupListQuery request, IQueryable<CardTopupRequest> query)
    {
        if (!string.IsNullOrEmpty(request.Name))
        {
            query = query.Where(t => t.Name.Contains(request.Name));
        }
        if (!string.IsNullOrEmpty(request.BinNumber))
        {
            query = query.Where(t => t.CardNumber.Contains(request.BinNumber));
        }
        if (!string.IsNullOrEmpty(request.WalletNumber))
        {
            query = query.Where(t => t.WalletNumber.Contains(request.WalletNumber));
        }
        return query;
    }

    private static IQueryable<CardTopupRequest> ApplyEnumFilter(GetTopupListQuery request, IQueryable<CardTopupRequest> query)
    {
        if (request.PaymentProviderType.HasValue)
        {
            query = query.Where(t => t.PaymentProviderType == request.PaymentProviderType);
        }
        if (request.CardType.HasValue)
        {
            query = query.Where(t => t.CardType == request.CardType);
        }
        return query;
    }
}
