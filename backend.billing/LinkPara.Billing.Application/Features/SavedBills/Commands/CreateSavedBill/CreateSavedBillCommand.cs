using LinkPara.Billing.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.Billing.Application.Features.SavedBills.Commands.CreateSavedBill;

public class CreateSavedBillCommand : IRequest
{
    public Guid InstitutionId { get; set; }
    public string SubscriberNumber1 { get; set; }
    public string SubscriberNumber2 { get; set; }
    public string SubscriberNumber3 { get; set; }
    public string BillName { get; set; }
}

public class CreateSavedBillCommandHandler : IRequestHandler<CreateSavedBillCommand>
{
    private readonly ISavedBillService _savedBillService;

    public CreateSavedBillCommandHandler(ISavedBillService savedBillService)
    {
        _savedBillService = savedBillService;
    }

    public async Task<Unit> Handle(CreateSavedBillCommand request, CancellationToken cancellationToken)
    {
        await _savedBillService.SaveAsync(request);

        return Unit.Value;
    }
}