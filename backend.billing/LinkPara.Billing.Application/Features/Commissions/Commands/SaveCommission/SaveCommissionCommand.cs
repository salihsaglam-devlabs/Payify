using LinkPara.Billing.Application.Commons.Attributes;
using LinkPara.Billing.Application.Commons.Interfaces;
using LinkPara.Billing.Domain.Enums;
using LinkPara.SharedModels.Persistence;
using MediatR;

namespace LinkPara.Billing.Application.Features.Commissions.Commands.SaveCommission;

public class SaveCommissionCommand : IRequest
{
    [Audit]
    public Guid CommissionId { get; set; }
    public Guid InstitutionId { get; set; }
    public RecordStatus RecordStatus { get; set; }
    public PaymentSource PaymentType { get; set; }
    [Audit]
    public decimal Rate { get; set; }
    public decimal Fee { get; set; }
    public int MinValue { get; set; }
    public int MaxValue { get; set; }
}

public class SaveCommissionCommandHandler : IRequestHandler<SaveCommissionCommand>
{
    private readonly ICommissionService _commissionService;

    public SaveCommissionCommandHandler(ICommissionService commissionService)
    {
        _commissionService = commissionService;
    }

    public async Task<Unit> Handle(SaveCommissionCommand request, CancellationToken cancellationToken)
    {
        await _commissionService.UpdateAsync(request);

        return Unit.Value;
    }
}