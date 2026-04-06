using LinkPara.Emoney.Application.Commons.Models;
using LinkPara.Emoney.Application.Features.Wallets.Commands.Transfer;
using LinkPara.Emoney.Application.Features.Wallets.Commands.WithdrawRequests;
using LinkPara.Emoney.Application.Features.Wallets.Queries;
using LinkPara.Emoney.Application.Features.Wallets.Queries.TransferPreview;
using LinkPara.Emoney.Application.Features.Wallets.Queries.WithdrawPreview;

namespace LinkPara.Emoney.Application.Commons.Interfaces;

public interface ITransferService
{
    Task<MoneyTransferResponse> TransferAsync(TransferCommand request, CancellationToken cancellationToken);
    Task<TransferPreviewResponse> TransferPreviewAsync(TransferPreviewQuery request);
    Task<MoneyTransferResponse> WithdrawAsync(WithdrawRequestCommand request, CancellationToken cancellationToken);
    Task<WithdrawPreviewResponse> WithdrawPreviewAsync(WithdrawPreviewQuery request);
    Task ValidateCurrentAndSenderUser(string senderWalletNumber);
    Task ValidateUserAndSenderUser(string senderWalletNumber, Guid userId);
}