using LinkPara.ApiGateway.Commons.Helpers;

namespace LinkPara.ApiGateway.Services.CampaignManagement.Models.Requests;

public class IWalletGenerateQrCodeRequest
{
}

public class IWalletGenerateQrCodeServiceRequest : IWalletGenerateQrCodeRequest , IHasUserId
{
    public Guid UserId { get; set; }
    public string WalletNumber { get; set; }
}