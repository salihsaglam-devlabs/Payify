using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Domain.Enums;
using MediatR;

namespace LinkPara.Emoney.Application.Features.WalletBlockages.Commands;

public class AddWalletBlockageCommand : IRequest
{
    public string WalletNumber { get; set; }
    public decimal CashBlockageAmount { get; set; }
    public decimal CreditBlockageAmount { get; set; }
    public WalletBlockageOperationType OperationType { get; set; }
    public string BlockageType { get; set; }
    public string BlockageDescription { get; set; }
    public DateTime BlockageStartDate { get; set; }
    public DateTime? BlockageEndDate { get; set; }
}

public class AddWalletBlockageCommandHandler : IRequestHandler<AddWalletBlockageCommand>
{
    
    private readonly IWalletBlockageService _WalletBlockageService;

    public AddWalletBlockageCommandHandler(IWalletBlockageService WalletBlockageService)
    {
        _WalletBlockageService = WalletBlockageService;
    }

    public async Task<Unit> Handle(AddWalletBlockageCommand request, CancellationToken cancellationToken)
    {
        await _WalletBlockageService.AddWalletBlockageAsync(request);
        return Unit.Value;
    }
}