using LinkPara.Billing.Application.Commons.Interfaces;
using LinkPara.Billing.Domain.Enums;
using MediatR;

namespace LinkPara.Billing.Application.Features.Commissions.Queries.GetByDetail;

public class GetByDetailQuery : IRequest<CommissionDto>
{
    public Guid InstitutionId { get; set; }
    public decimal Amount { get; set; }
    public PaymentSource PaymentSource { get; set; }
}

public class GetByInstitutionQueryHandler : IRequestHandler<GetByDetailQuery, CommissionDto>
{
    private readonly ICommissionService _commissionService;

    public GetByInstitutionQueryHandler(ICommissionService commissionService)
    {
        _commissionService = commissionService;
    }

    public async Task<CommissionDto> Handle(GetByDetailQuery request, CancellationToken cancellationToken)
    {
        return await _commissionService.GetByInstitutionAsync(request);
    }
}