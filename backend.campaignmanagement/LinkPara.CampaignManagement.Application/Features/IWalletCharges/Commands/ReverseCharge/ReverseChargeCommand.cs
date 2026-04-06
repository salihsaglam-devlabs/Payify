using LinkPara.CampaignManagement.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.CampaignManagement.Application.Features.IWalletCharges.Commands.ReverseCharge;

public class ReverseChargeCommand : IRequest
{
    public Guid ProcessGuid { get; set; }
    public decimal ReversedAmount { get; set; }
    public decimal CashBackAmount { get; set; }
}

public class ReverseChargeCommandHandler : IRequestHandler<ReverseChargeCommand>
{
    private readonly IIWalletChargeService _walletChargeService;

    public ReverseChargeCommandHandler(IIWalletChargeService walletChargeService)
    {
        _walletChargeService = walletChargeService;
    }

    public async Task<Unit> Handle(ReverseChargeCommand request, CancellationToken cancellationToken)
    {
        await _walletChargeService.ReverseChargeAsync(request);
        return Unit.Value;
    }
}
