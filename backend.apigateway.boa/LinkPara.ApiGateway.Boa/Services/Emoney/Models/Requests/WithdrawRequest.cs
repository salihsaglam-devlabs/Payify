using LinkPara.ApiGateway.Boa.Commons.Helpers;

namespace LinkPara.ApiGateway.Boa.Services.Emoney.Models.Requests;

public class WithdrawRequest
{
    public decimal Amount { get; set; }
    public string ReceiverIBAN { get; set; }
    public string ReceiverName { get; set; }
    public string Description { get; set; }
    public string PaymentType { get; set; }
    public string WalletNumber { get; set; }
    public string TransactionToken { get; set; }
    public string IdempotentKey { get; set; }
}

public class WithdrawServiceRequest : WithdrawRequest, IHasUserId
{
    public Guid UserId { get; set; }
}
