using LinkPara.ApiGateway.CorporateWallet.Commons.Helpers;

namespace LinkPara.ApiGateway.CorporateWallet.Services.Emoney.Models.Requests;

public class TransferPreviewRequest
{
    public string SenderWalletNumber { get; set; }
    public string ReceiverWalletNumber { get; set; }
    public string Description { get; set; }
    public decimal Amount { get; set; }
}

public class TransferPreviewServiceRequest : TransferPreviewRequest, IHasUserId
{
    public Guid UserId { get; set; }
}