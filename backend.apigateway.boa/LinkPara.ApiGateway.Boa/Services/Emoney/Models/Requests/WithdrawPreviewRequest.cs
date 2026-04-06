using LinkPara.ApiGateway.Boa.Commons.Helpers;

namespace LinkPara.ApiGateway.Boa.Services.Emoney.Models.Requests;

public class WithdrawPreviewRequest
{
    public decimal Amount { get; set; }
    public string ReceiverIBAN { get; set; }
    public string ReceiverName { get; set; }
    public string Description { get; set; }
    public string WalletNumber { get; set; }
    public string PaymentType { get; set; }
}

public class WithdrawPreviewServiceRequest : WithdrawPreviewRequest, IHasUserId
{
    public Guid UserId { get; set; }
}
