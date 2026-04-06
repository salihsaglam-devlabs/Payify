using LinkPara.Billing.Application.Commons.Interfaces;
using LinkPara.Billing.Domain.Enums;
using MediatR;

namespace LinkPara.Billing.Application.Features.Commissions.Commands.CreateCommission;

public class CreateCommissionQuery : IRequest
{
    public Guid VendorId { get; set; }
    public PaymentSource PaymentType { get; set; }
    public Guid InstitutionId { get; set; }
    public decimal Rate { get; set; }
    public decimal Fee { get; set; }
    public decimal MinValue { get; set; }
    public decimal MaxValue { get; set; }
}

public class CreateCommissionQueryHandler : IRequestHandler<CreateCommissionQuery, Unit>
{
    private readonly ICommissionService _commissionService;

    public CreateCommissionQueryHandler(ICommissionService commissionService)
    {
        _commissionService = commissionService;
    }

    public async Task<Unit> Handle(CreateCommissionQuery request, CancellationToken cancellationToken)
    {
        await _commissionService.AddAsync(request);

        return Unit.Value;
    }
}