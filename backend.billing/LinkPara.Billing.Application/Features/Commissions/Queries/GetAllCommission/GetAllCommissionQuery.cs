using LinkPara.Billing.Application.Commons.Interfaces;
using LinkPara.Billing.Domain.Enums;
using LinkPara.SharedModels.Pagination;
using MediatR;

namespace LinkPara.Billing.Application.Features.Commissions.Queries.GetAllCommission;

public class GetAllCommissionQuery : SearchQueryParams, IRequest<PaginatedList<CommissionDto>>
{
    public Guid? CommissionId { get; set; }
    public Guid? InstitutionId { get; set; }
    public Guid? VendorId { get; set; }
    public PaymentSource? PaymentType { get; set; }
}

public class GetAllCommissionQueryHandler : IRequestHandler<GetAllCommissionQuery, PaginatedList<CommissionDto>>
{
    private readonly ICommissionService _commissionService;

    public GetAllCommissionQueryHandler(ICommissionService commissionService)
    {
        _commissionService = commissionService;
    }

    public async Task<PaginatedList<CommissionDto>> Handle(GetAllCommissionQuery request, CancellationToken cancellationToken)
    {
        return await _commissionService.GetAllAsync(request);
    }
}