
using LinkPara.CampaignManagement.Application.Features.IWalletQrCodes;

namespace LinkPara.CampaignManagement.Application.Commons.Interfaces;

public interface IIWalletQrCodeService
{
    Task<IWalletQrCodeDto> GenerateQrCodeAsync(Guid userId, string walletNumber);
}
