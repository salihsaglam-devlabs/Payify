using LinkPara.Emoney.Application.Commons.Enums;
using LinkPara.Emoney.Application.Features.Topups;
using LinkPara.Emoney.Application.Features.Topups.Commands.TopupProcess;
using LinkPara.Emoney.Domain.Entities;

namespace LinkPara.Emoney.Application.Commons.Interfaces;

public interface ITopupService
{
    Task<TopupProcessResponse> TopupProcessAsync(TopupProcessCommand topupProcess, Wallet wallet, CardTopupRequest cardTopupRequest, string cardHolderName, decimal amount);
    Task TopupReverseAsync(CardTopupRequest request, CardTopupRequestStatus cardTopupRequestStatus, Wallet wallet);
}
