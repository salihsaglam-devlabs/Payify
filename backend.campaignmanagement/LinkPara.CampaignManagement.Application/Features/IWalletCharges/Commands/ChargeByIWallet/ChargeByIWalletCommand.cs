using LinkPara.CampaignManagement.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.CampaignManagement.Application.Features.IWalletCharges.Commands.ChargeByIWallet;

public class ChargeByIWalletCommand : IRequest<Guid>
{
    public string WalletId { get; set; }
    public int TerminalId { get; set; }
    public string TerminalName { get; set; }
    public int CurrencyCode { get; set; }
    public int QrCode { get; set; }
    public decimal Amount { get; set; }
}

public class ChargeByIWalletCommandHandler : IRequestHandler<ChargeByIWalletCommand, Guid>
{
    private readonly IIWalletChargeService _iwalletChargeService;

    public ChargeByIWalletCommandHandler(IIWalletChargeService iwalletChargeService)
    {
        _iwalletChargeService = iwalletChargeService;
    }

    public async Task<Guid> Handle(ChargeByIWalletCommand request, CancellationToken cancellationToken)
    {
        return await _iwalletChargeService.SaveChargeAsync(request);
    }
}
