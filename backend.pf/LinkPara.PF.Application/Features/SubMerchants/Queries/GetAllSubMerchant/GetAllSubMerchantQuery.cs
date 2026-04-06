using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using MediatR;

namespace LinkPara.PF.Application.Features.SubMerchants.Queries.GetAllSubMerchant;

public class GetAllSubMerchantQuery : SearchQueryParams, IRequest<PaginatedList<SubMerchantDto>>
{
    public Guid? MerchantId { get; set; } 
    public int? CityCode { get; set; }
    public DateTime? CreateDateStart { get; set; }
    public DateTime? CreateDateEnd { get; set; }
    public RecordStatus? RecordStatus { get; set; }
    public string Number { get; set; }
    public bool? IsManuelPaymentPageAllowed { get; set; }
    public bool? IsLinkPaymentPageAllowed { get; set; }
    public bool? IsOnUsPaymentPageAllowed { get; set; }
    public bool? PaymentReverseAllowed { get; set; }
    public bool? PaymentReturnAllowed { get; set; }
}

public class GetAllSubMerchantQueryHandler : IRequestHandler<GetAllSubMerchantQuery, PaginatedList<SubMerchantDto>>
{
    private readonly ISubMerchantService _subMerchantService;

    public GetAllSubMerchantQueryHandler(ISubMerchantService subMerchantService)
    {
        _subMerchantService = subMerchantService;
    }

    public async Task<PaginatedList<SubMerchantDto>> Handle(GetAllSubMerchantQuery request, CancellationToken cancellationToken)
    {
        return await _subMerchantService.GetListAsync(request);
    }
}
