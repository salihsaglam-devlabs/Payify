using AutoMapper;
using LinkPara.MappingExtensions.Mapping;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.PF.Application.Features.MerchantDues.Queries.GetFilterMerchantDue;

public class GetFilterMerchantDueQuery : SearchQueryParams, IRequest<PaginatedList<MerchantDueDto>>
{
    public Guid? MerchantId { get; set; }
    public Guid? DueProfileId { get; set; }
    public DueType? DueType { get; set; }
    public DateTime? LastExecutionDateStart { get; set; }
    public DateTime? LastExecutionDateEnd { get; set; }
    public int? ExecutionCountBigger { get; set; }
    public int? ExecutionCountLower { get; set; }
    public string Title { get; set; }
    public decimal? AmountBiggerThan { get; set; }
    public decimal? AmountSmallerThan { get; set; }
    public int? Currency { get; set; }
    public TimeInterval? OccurenceInterval { get; set; }
    public bool? IsDefault { get; set; }
    public RecordStatus? RecordStatus { get; set; }
    public DateTime? CreateDateStart { get; set; }
    public DateTime? CreateDateEnd { get; set; }
}

public class GetFilterMerchantDueQueryHandler : IRequestHandler<GetFilterMerchantDueQuery, PaginatedList<MerchantDueDto>>
{
    private readonly IGenericRepository<MerchantDue> _merchantDueRepository;
    private readonly IMapper _mapper;

    public GetFilterMerchantDueQueryHandler(IGenericRepository<MerchantDue> merchantDueRepository, IMapper mapper)
    {
        _merchantDueRepository = merchantDueRepository;
        _mapper = mapper;
    }
    public async Task<PaginatedList<MerchantDueDto>> Handle(GetFilterMerchantDueQuery request, CancellationToken cancellationToken)
    {
        var merchantDueList = _merchantDueRepository
            .GetAll()
            .Include(b => b.DueProfile)
            .AsQueryable();

        if (request.RecordStatus is not null)
        {
            merchantDueList = merchantDueList.Where(b => b.RecordStatus == request.RecordStatus);
        }
        
        if (request.MerchantId is not null)
        {
            merchantDueList = merchantDueList.Where(b => b.MerchantId
                                                         == request.MerchantId);
        }
        
        if (request.DueProfileId is not null)
        {
            merchantDueList = merchantDueList.Where(b => b.DueProfileId
                                                         == request.DueProfileId);
        }
        
        if (request.DueType is not null)
        {
            merchantDueList = merchantDueList.Where(b => b.DueProfile.DueType
                                                         == request.DueType);
        }

        if (request.LastExecutionDateStart is not null)
        {
            merchantDueList = merchantDueList.Where(b => b.LastExecutionDate
                                                         >= request.LastExecutionDateStart);
        }

        if (request.LastExecutionDateEnd is not null)
        {
            merchantDueList = merchantDueList.Where(b => b.LastExecutionDate
                                                         <= request.LastExecutionDateEnd);
        }
        
        if (request.ExecutionCountBigger is not null)
        {
            merchantDueList = merchantDueList.Where(b => b.TotalExecutionCount
                                                         >= request.ExecutionCountBigger);
        }

        if (request.ExecutionCountLower is not null)
        {
            merchantDueList = merchantDueList.Where(b => b.TotalExecutionCount
                                                         <= request.ExecutionCountLower);
        }

        if (!string.IsNullOrEmpty(request.Title))
        {
            merchantDueList =
                merchantDueList.Where(b => b.DueProfile.Title.ToLower().Contains(request.Title.ToLower()));
        }

        if (request.AmountBiggerThan is not null)
        {
            merchantDueList = merchantDueList.Where(b => b.DueProfile.Amount >= request.AmountBiggerThan);
        }
        
        if (request.AmountSmallerThan is not null)
        {
            merchantDueList = merchantDueList.Where(b => b.DueProfile.Amount <= request.AmountSmallerThan);
        }
        
        if (request.Currency is not null)
        {
            merchantDueList = merchantDueList.Where(b => b.DueProfile.Currency == request.Currency);
        }
        
        if (request.OccurenceInterval is not null)
        {
            merchantDueList = merchantDueList.Where(b => b.DueProfile.OccurenceInterval == request.OccurenceInterval);
        }
        
        if (request.IsDefault is not null)
        {
            merchantDueList = merchantDueList.Where(b => b.DueProfile.IsDefault == request.IsDefault);
        }
        
        if (request.CreateDateStart is not null)
        {
            merchantDueList = merchantDueList.Where(b => b.CreateDate
                                                         >= request.CreateDateStart);
        }

        if (request.CreateDateEnd is not null)
        {
            merchantDueList = merchantDueList.Where(b => b.CreateDate
                                                         <= request.CreateDateEnd);
        }

        return await merchantDueList
            .PaginatedListWithMappingAsync<MerchantDue,MerchantDueDto>(_mapper, request.Page, request.Size, request.OrderBy, request.SortBy);
    }
}