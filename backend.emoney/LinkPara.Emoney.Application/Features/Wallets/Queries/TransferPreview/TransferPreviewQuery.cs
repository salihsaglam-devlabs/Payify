using LinkPara.Emoney.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.Emoney.Application.Features.Wallets.Queries.TransferPreview;

public class TransferPreviewQuery : IRequest<TransferPreviewResponse>
{
    public string SenderWalletNumber { get; set; }
    public string ReceiverWalletNumber { get; set; }
    public string Description { get; set; }
    public decimal Amount { get; set; }
    public string UserId { get; set; }
    public string PaymentType { get; set; }
}

public class TransferPreviewQueryHandler : IRequestHandler<TransferPreviewQuery, TransferPreviewResponse>
{
    private readonly ITransferService _transferService;
    
    public TransferPreviewQueryHandler(ITransferService transferService)
    {
        _transferService = transferService;
    }

    public async Task<TransferPreviewResponse> Handle(TransferPreviewQuery request, CancellationToken cancellationToken)
    {
        await _transferService.ValidateCurrentAndSenderUser(request.SenderWalletNumber);
        
        return await _transferService.TransferPreviewAsync(request);
    }
}
