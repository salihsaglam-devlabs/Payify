using LinkPara.Billing.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.Billing.Application.Features.Commissions.Queries.GetById;

public class GetCommissionByIdQuery : IRequest<CommissionDto>
{
    public Guid CommissionId { get; set; }
}

public class GetCommissionByIdQueryHandler : IRequestHandler<GetCommissionByIdQuery, CommissionDto>
{
    private readonly ICommissionService _commissionService;

    public GetCommissionByIdQueryHandler(ICommissionService commissionService)
    {
        _commissionService = commissionService;
    }

    public async Task<CommissionDto> Handle(GetCommissionByIdQuery request, CancellationToken cancellationToken)
    {
        return await _commissionService.GetByIdAsync(request.CommissionId);
    }
}