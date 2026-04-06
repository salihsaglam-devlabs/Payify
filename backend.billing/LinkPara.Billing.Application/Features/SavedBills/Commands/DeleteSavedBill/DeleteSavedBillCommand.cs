using LinkPara.Billing.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.Billing.Application.Features.SavedBills.Commands.DeleteSavedBill;

public class DeleteSavedBillCommand : IRequest
{
    public Guid Id { get; set; }
}

public class DeleteSavedBillCommandHandler : IRequestHandler<DeleteSavedBillCommand>
{
    private readonly ISavedBillService _savedBillService;

    public DeleteSavedBillCommandHandler(ISavedBillService savedBillService)
    {
        _savedBillService = savedBillService;
    }

    public async Task<Unit> Handle(DeleteSavedBillCommand request, CancellationToken cancellationToken)
    {
        await _savedBillService.DeleteAsync(request);

        return Unit.Value;
    }
}