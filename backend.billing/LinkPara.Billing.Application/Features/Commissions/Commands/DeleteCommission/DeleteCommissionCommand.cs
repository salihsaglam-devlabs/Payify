using LinkPara.Billing.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.Billing.Application.Features.Commissions.Commands.DeleteCommission;

public class DeleteCommissionCommand : IRequest
{
    public Guid CommissionId { get; set; }
}

public class DeleteCommissionCommandHandler : IRequestHandler<DeleteCommissionCommand>
{
    private readonly ICommissionService _commissionService;

    public DeleteCommissionCommandHandler(ICommissionService commissionService)
    {
        _commissionService = commissionService;
    }
   
    public async Task<Unit> Handle(DeleteCommissionCommand request, CancellationToken cancellationToken)
    {
        await _commissionService.DeleteAsync(request.CommissionId);
        
        return Unit.Value;
    }
}