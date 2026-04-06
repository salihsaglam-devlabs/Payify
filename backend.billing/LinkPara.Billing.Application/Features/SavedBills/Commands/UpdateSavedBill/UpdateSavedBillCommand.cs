using LinkPara.Billing.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.Billing.Application.Features.SavedBills.Commands.UpdateSavedBill;

public class UpdateSavedBillCommand : IRequest
{
    public Guid Id { get; set; }
    public string BillName { get; set; }
}

public class UpdateSavedBillQueryHandler : IRequestHandler<UpdateSavedBillCommand>
{
    private readonly ISavedBillService _savedBillService;

    public UpdateSavedBillQueryHandler(ISavedBillService savedBillService)
    {
        _savedBillService = savedBillService;
    }

    public async Task<Unit> Handle(UpdateSavedBillCommand request, CancellationToken cancellationToken)
    {
        await _savedBillService.UpdateAsync(request);

        return Unit.Value;
    }
}