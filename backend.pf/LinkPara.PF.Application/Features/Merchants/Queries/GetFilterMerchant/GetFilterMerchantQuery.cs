using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Pagination;
using MediatR;

namespace LinkPara.PF.Application.Features.Merchants.Queries.GetFilterMerchant;

public class GetFilterMerchantQuery : SearchQueryParams, IRequest<PaginatedList<MerchantDto>>
{
    public Guid? MerchantId { get; set; }
    public string MerchantNumber { get; set; }
    public string MerchantName { get; set; }
    public MerchantType? MerchantType { get; set; }
    public Guid? ParentMerchantId { get; set; }
    public DateTime? CreateDateStart { get; set; }
    public DateTime? CreateDateEnd { get; set; }
    public ApplicationChannel? ApplicationChannel { get; set; }
    public CompanyType? CompanyType { get; set; }
    public MerchantStatus? MerchantStatus { get; set; }
    public IntegrationMode? IntegrationMode { get; set; }
    public PostingPaymentChannel? PostingPaymentChannel { get; set; }
    public int? CountryCode { get; set; }
    public int? CityCode { get; set; }
    public bool? IsBlockage { get; set; }
    public string PricingProfileNumber { get; set; }
    public bool? IsReturnAllowed { get; set; }
    public bool? IsReverseAllowed { get; set; }
    public bool? InsurancePaymentAllowed { get; set; }
    public bool? IsInstallmentAllowed { get; set; }
    public bool? IsPreAuthAllowed { get; set; }
    public bool? IsInternationalCardAllowed { get; set; }
    public string MccCode { get; set; }
    public string NaceCode { get; set; }
    public PosType? PosType { get; set; }
    public int? MoneyTransferStartHourStart { get; set; }
    public int? MoneyTransferStartHourFinish { get; set; }
    public int? MoneyTransferStartMinuteStart { get; set; }
    public int? MoneyTransferStartMinuteFinish { get; set; }
}

public class GetFilterMerchantQueryHandler : IRequestHandler<GetFilterMerchantQuery, PaginatedList<MerchantDto>>
{
    private readonly IMerchantService _merchantService;

    public GetFilterMerchantQueryHandler(IMerchantService merchantService)
    {
        _merchantService = merchantService;
    }
    public async Task<PaginatedList<MerchantDto>> Handle(GetFilterMerchantQuery request, CancellationToken cancellationToken)
    {
        return await _merchantService.GetFilterListAsync(request);
    }
}
