using LinkPara.ApiGateway.CorporateWallet.Commons.Helpers;

namespace LinkPara.ApiGateway.CorporateWallet.Services.Emoney.Models.Requests;

public class WithdrawPreviewRequest
{
    public decimal Amount { get; set; }
    public string ReceiverIBAN { get; set; }
    public string ReceiverName { get; set; }
    public string Description { get; set; }
    public string WalletNumber { get; set; }
}

public class WithdrawPreviewServiceRequest : WithdrawPreviewRequest, IHasUserId
{
    public Guid UserId { get; set; }
}
