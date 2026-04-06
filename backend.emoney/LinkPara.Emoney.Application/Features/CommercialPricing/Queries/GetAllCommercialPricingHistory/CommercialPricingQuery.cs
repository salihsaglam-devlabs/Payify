using LinkPara.Emoney.Domain.Entities;
using LinkPara.Emoney.Domain.Enums;
using LinkPara.MappingExtensions.Mapping;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using MediatR;

namespace LinkPara.Emoney.Application.Features.CommercialPricing.Queries.GetAllCommercialPricingHistory;

public class CommercialPricingQuery : SearchQueryParams, IRequest<PaginatedList<PricingCommercialDto>>
{
    public PricingCommercialType? PricingCommercialType { get; set; }
    public string CurrencyCode { get; set; }
    public DateTime? ActivationDate { get; set; }
    public PricingCommercialStatus? PricingCommercialStatus { get; set; }
}

public class CommercialPricingQueryHandler : IRequestHandler<CommercialPricingQuery, PaginatedList<PricingCommercialDto>>
{
    private readonly IGenericRepository<PricingCommercial> _commercialPricing;

    public CommercialPricingQueryHandler(
        IGenericRepository<PricingCommercial> commercialPricing)
    {
        _commercialPricing = commercialPricing;
    }
    public async Task<PaginatedList<PricingCommercialDto>> Handle(CommercialPricingQuery request, CancellationToken cancellationToken)
    {
        
        var historyList = _commercialPricing
            .GetAll()
            .Select(s => 
                new PricingCommercialDto 
                {
                    Id = s.Id,
                    MaxDistinctSenderCount = s.MaxDistinctSenderCount,
                    MaxDistinctSenderCountWithAmount = s.MaxDistinctSenderCountWithAmount,
                    MaxDistinctSenderAmount = s.MaxDistinctSenderAmount,
                    PricingCommercialType = s.PricingCommercialType,
                    ActivationDate = s.ActivationDate,
                    CommissionRate = s.CommissionRate,
                    CurrencyCode = s.CurrencyCode,
                    PricingCommercialStatus = s.PricingCommercialStatus
                })
            .AsQueryable();
       
        if (request.PricingCommercialStatus is not null)
        {
            historyList = historyList.Where(s => s.PricingCommercialStatus == request.PricingCommercialStatus);
        }

        if (request.PricingCommercialType is not null)
        {
            historyList = historyList.Where(s => s.PricingCommercialType == request.PricingCommercialType);
        }

        if (request.ActivationDate is not null)
        {
            historyList = historyList.Where(s => s.ActivationDate == request.ActivationDate);
        }

        if (!string.IsNullOrEmpty(request.CurrencyCode))
        {
            historyList = historyList.Where(s => s.CurrencyCode == request.CurrencyCode);
        }

        return await historyList.PaginatedListAsync(request.Page, request.Size, OrderByStatus.Desc, $"ActivationDate");
    }
}
