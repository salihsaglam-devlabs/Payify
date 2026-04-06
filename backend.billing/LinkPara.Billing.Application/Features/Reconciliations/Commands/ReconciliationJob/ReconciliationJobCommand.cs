using LinkPara.Billing.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.Billing.Application.Features.Reconciliations.Commands.ReconciliationJob;

public class ReconciliationJobCommand : IRequest<ReconciliationJobResponseDto>
{
    public Guid VendorId { get; set; }
    public DateTime ReconciliationDate { get; set; }
}

public class ReconciliationJobCommandHandler : IRequestHandler<ReconciliationJobCommand, ReconciliationJobResponseDto>
{
    private readonly IBillingService _billingService;

    public ReconciliationJobCommandHandler(IBillingService billingService)
    {
        _billingService = billingService;
    }

    public async Task<ReconciliationJobResponseDto> Handle(ReconciliationJobCommand request, CancellationToken cancellationToken)
    {
        var response = await _billingService.DoReconciliationAsync(request);

        return new ReconciliationJobResponseDto
        {
            IsSuccess = response.Response
        };
    }
}