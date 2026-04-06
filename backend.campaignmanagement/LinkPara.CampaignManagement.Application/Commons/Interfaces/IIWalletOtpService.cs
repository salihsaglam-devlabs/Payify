
using LinkPara.CampaignManagement.Application.Features.IWalletOtpCodes;
using LinkPara.CampaignManagement.Application.Features.IWalletOtpCodes.Commands;

namespace LinkPara.CampaignManagement.Application.Commons.Interfaces;

public interface IIWalletOtpService
{
    Task<SendIWalletOtpCodeResponseDto> NotifyOtpCodeAsync(SendIWalletOtpCodeCommand request, CancellationToken cancellationToken);
}
