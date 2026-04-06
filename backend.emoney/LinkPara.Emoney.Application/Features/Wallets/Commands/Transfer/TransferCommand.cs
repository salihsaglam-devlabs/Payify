using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Application.Commons.Models;
using MediatR;

namespace LinkPara.Emoney.Application.Features.Wallets.Commands.Transfer;

public class TransferCommand : IRequest<MoneyTransferResponse>
{
    public string SenderWalletNumber { get; set; }
    public string ReceiverWalletNumber { get; set; }
    public string Description { get; set; }
    public decimal Amount { get; set; }
    public string UserId { get; set; }
    public string PaymentType { get; set; }
    public string IdempotentKey { get; set; }
}
public class TransferCommandHandler : IRequestHandler<TransferCommand, MoneyTransferResponse>
{
    private readonly ITransferService _transferService;

    public TransferCommandHandler(ITransferService transferService)
    {
        _transferService = transferService;
    }
    
    public async Task<MoneyTransferResponse> Handle(TransferCommand request, CancellationToken cancellationToken)
    {
        await _transferService.ValidateCurrentAndSenderUser(request.SenderWalletNumber);
        
        return await _transferService.TransferAsync(request, cancellationToken);
    }
}