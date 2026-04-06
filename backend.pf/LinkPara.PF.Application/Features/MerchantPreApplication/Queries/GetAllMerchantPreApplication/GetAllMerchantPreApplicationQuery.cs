using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Models.MerchantPreApplication;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Pagination;
using MediatR;

namespace LinkPara.PF.Application.Features.MerchantPreApplication.Queries.GetAllMerchantPreApplication;

public class GetAllMerchantPreApplicationQuery : SearchQueryParams, IRequest<PaginatedList<MerchantPreApplicationDto>>
{
    public string? FullName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? ResponsiblePerson { get; set; }
    public string? Website { get; set; }
    public DateTime? CreateDateStart { get; set; }
    public DateTime? CreateDateEnd { get; set; }
    public PosProductType? ProductType { get; set; }
    public ApplicationStatus? ApplicationStatus { get; set; }
    public MonthlyTurnover? MonthlyTurnover { get; set; }
}
public class GetAllPendingPosApplicationQueryHandler : IRequestHandler<GetAllMerchantPreApplicationQuery, PaginatedList<MerchantPreApplicationDto>>
{
    private readonly IMerchantPreApplicationService _merchantPreApplicationService;

    public GetAllPendingPosApplicationQueryHandler(IMerchantPreApplicationService merchantPreApplicationService)
    {
        _merchantPreApplicationService = merchantPreApplicationService;
    }

    public async Task<PaginatedList<MerchantPreApplicationDto>> Handle(GetAllMerchantPreApplicationQuery request, CancellationToken cancellationToken)
    {
        return await _merchantPreApplicationService.GetFilterAsync(request);
    }
}