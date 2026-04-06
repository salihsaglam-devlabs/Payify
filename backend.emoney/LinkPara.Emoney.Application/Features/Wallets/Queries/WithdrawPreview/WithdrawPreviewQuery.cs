using LinkPara.Emoney.Application.Commons.Exceptions;
using LinkPara.Emoney.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.Emoney.Application.Features.Wallets.Queries.WithdrawPreview;

public class WithdrawPreviewQuery : IRequest<WithdrawPreviewResponse>
{
    public decimal Amount { get; set; }
    public string ReceiverIBAN { get; set; }
    public string ReceiverName { get; set; }
    public string Description { get; set; }
    public string WalletNumber { get; set; }
    public Guid UserId { get; set; }
    public string PaymentType { get; set; }
}

public class WithdrawPreviewQueryHandler : IRequestHandler<WithdrawPreviewQuery, WithdrawPreviewResponse>
{
    private readonly ITransferService _moneyTransferService;
    private readonly IIbanBlacklistService _ibanBlacklistService;

    public WithdrawPreviewQueryHandler(
        ITransferService moneyTransferService,
        IIbanBlacklistService ibanBlacklistService)
    {
        _moneyTransferService = moneyTransferService;
        _ibanBlacklistService = ibanBlacklistService;
    }

    public async Task<WithdrawPreviewResponse> Handle(WithdrawPreviewQuery request, CancellationToken cancellationToken)
    {
        await _moneyTransferService.ValidateCurrentAndSenderUser(request.WalletNumber);

        var ibanBlacklisted = await _ibanBlacklistService.IsBlacklistedAsync(request.ReceiverIBAN);

        if (ibanBlacklisted)
        {
            throw new IbanBlacklistedException(request.ReceiverIBAN);
        }

        return await _moneyTransferService.WithdrawPreviewAsync(request);
    }
}