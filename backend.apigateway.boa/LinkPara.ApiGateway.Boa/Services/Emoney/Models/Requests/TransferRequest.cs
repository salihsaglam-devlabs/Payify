using LinkPara.ApiGateway.Boa.Commons.Helpers;

namespace LinkPara.ApiGateway.Boa.Services.Emoney.Models.Requests;

public class TransferRequest
{
    public string SenderWalletNumber { get; set; }
    public string ReceiverWalletNumber { get; set; }
    public string Description { get; set; }
    public decimal Amount { get; set; }
    public string PaymentType { get; set; }
    public string IdempotentKey { get; set; }
}

public class TransferServiceRequest : TransferRequest, IHasUserId
{
    public Guid UserId { get; set; }
}
