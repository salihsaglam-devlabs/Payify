using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Domain.Enums;
using LinkPara.SharedModels.Banking.Enums;
using LinkPara.SharedModels.Pagination;
using MediatR;

namespace LinkPara.Emoney.Application.Features.PricingProfiles.Queries.GetPricingProfileList;

public class GetPricingProfileListQuery : SearchQueryParams, IRequest<PaginatedList<PricingProfileDto>>
{
    public int? BankCode { get; set; }
    public string CurrencyCode { get; set; }
    public TransferType? TransferType { get; set; }
    public PricingProfileStatus? Status { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public CardType? CardType { get; set; }
}

public class GetPricingProfileListQueryHandler : IRequestHandler<GetPricingProfileListQuery, PaginatedList<PricingProfileDto>>
{
    private readonly IPricingProfileService _service;

    public GetPricingProfileListQueryHandler(IPricingProfileService service)
    {
        _service = service;
    }

    public async Task<PaginatedList<PricingProfileDto>> Handle(GetPricingProfileListQuery request, CancellationToken cancellationToken)
    {
        return await _service.GetListAsync(request);
    }
}
