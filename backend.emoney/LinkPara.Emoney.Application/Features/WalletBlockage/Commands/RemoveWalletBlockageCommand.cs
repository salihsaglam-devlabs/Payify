using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Domain.Entities;
using MediatR;

namespace LinkPara.Emoney.Application.Features.WalletBlockages.Commands;

public class RemoveWalletBlockageCommand : IRequest<List<WalletBlockage>>
{
    public string WalletNumber { get; set; }
    public decimal CashBlockageAmount { get; set; }
    public decimal CreditBlockageAmount { get; set; }
    public string BlockageDescription { get; set; }
}

public class RemoveWalletBlockageCommandHandler : IRequestHandler<RemoveWalletBlockageCommand, List<WalletBlockage>>
{    
    private readonly IWalletBlockageService _walletBlockageService;

    public RemoveWalletBlockageCommandHandler(IWalletBlockageService walletBlockageService)
    {
        _walletBlockageService = walletBlockageService;
    }

    public async Task<List<WalletBlockage>> Handle(RemoveWalletBlockageCommand request, CancellationToken cancellationToken)
    {
        return await _walletBlockageService.RemoveExpiredBlockagesAsync();        
    }
}