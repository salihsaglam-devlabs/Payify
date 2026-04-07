using LinkPara.Emoney.Application.Commons.Exceptions;
using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Application.Commons.Models;
using LinkPara.Emoney.Domain.Enums;
using LinkPara.HttpProviders.MultiFactor;
using LinkPara.HttpProviders.MultiFactor.Models.PowerFactorModels.Enums;
using LinkPara.HttpProviders.MultiFactor.Models.PowerFactorModels.Request;
using MediatR;

namespace LinkPara.Emoney.Application.Features.Wallets.Commands.WithdrawRequests;

public class WithdrawRequestCommand : IRequest<MoneyTransferResponse>
{
    public decimal Amount { get; set; }
    public string ReceiverIBAN { get; set; }
    public string ReceiverName { get; set; }
    public string Description { get; set; }
    public string PaymentType { get; set; }
    public string WalletNumber { get; set; }
    public Guid UserId { get; set; }
    public string TransactionToken { get; set; }
}

public class WithdrawRequestCommandHandler : IRequestHandler<WithdrawRequestCommand, MoneyTransferResponse>
{
    private readonly ITransferService _transferService;
    private readonly IMultiFactorService _multiFactorService;
    private readonly IIbanBlacklistService _ibanBlacklistService;

    public WithdrawRequestCommandHandler(
        ITransferService transferService,
        IMultiFactorService multiFactorService,
        IIbanBlacklistService ibanBlacklistService)
    {
        _transferService = transferService;
        _multiFactorService = multiFactorService;
        _ibanBlacklistService = ibanBlacklistService;
    }

    public async Task<MoneyTransferResponse> Handle(WithdrawRequestCommand request, CancellationToken cancellationToken)
    { 
        if (!string.IsNullOrEmpty(request.TransactionToken))
        {
            var approval = await _multiFactorService.CheckTransactionApprovalAsync(new CheckTransactionApprovalRequest
            {
                TransactionToken = request.TransactionToken
            });

            if (!approval.Success)
            {
                throw new CheckTransactionApprovalException();
            }

            if (approval.Status != TransactionApprovalStatus.Approved)
            {
                throw new InitiateWithdrawException();
            }
        }

        await _transferService.ValidateCurrentAndSenderUser(request.WalletNumber);

        var ibanBlacklisted = await _ibanBlacklistService.IsBlacklistedAsync(request.ReceiverIBAN);

        if (ibanBlacklisted)
        {
            throw new IbanBlacklistedException(request.ReceiverIBAN);
        }

        var withdraw = await _transferService.WithdrawAsync(request, cancellationToken);

        if (!withdraw.Success)
        {
            throw new InitiateWithdrawException();
        }

        return withdraw;
    }
}