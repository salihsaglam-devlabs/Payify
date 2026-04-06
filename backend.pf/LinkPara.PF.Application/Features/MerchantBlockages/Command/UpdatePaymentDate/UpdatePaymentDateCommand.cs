using LinkPara.PF.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.PF.Application.Features.MerchantBlockages.Command.UpdatePaymentDate;

public class UpdatePaymentDateCommand : IRequest
{
    public Guid MerchantBlockageId { get; set; }
    public Guid PostBalanceId { get; set; }
    public DateTime PaymentDate { get; set; }
}

public class UpdatePaymentDateCommandHandler : IRequestHandler<UpdatePaymentDateCommand>
{
    private readonly IMerchantBlockageService _merchantBlockageService;

    public UpdatePaymentDateCommandHandler(IMerchantBlockageService merchantBlockageService)
    {
        _merchantBlockageService = merchantBlockageService;
    }
    public async Task<Unit> Handle(UpdatePaymentDateCommand request, CancellationToken cancellationToken)
    {
        await _merchantBlockageService.UpdatePaymentDateAsync(request);

        return Unit.Value;
    }
}
