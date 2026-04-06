using LinkPara.ApiGateway.Commons.Helpers;
using LinkPara.ApiGateway.Services.Emoney.Models.Enums;

namespace LinkPara.ApiGateway.Services.Emoney.Models.Requests;

public class MasterpassTopupProcessRequest
{
    public string WalletNumber { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; }
    public Guid UserId { get; set; }
    public Guid CardTopupRequestId { get; set; }
    public string OrderId { get; set; }
    public string AccountKey { get; set; }
    public int InstallmentCount { get; set; } = 0;
    public string RequestReferenceNo { get; set; }
    public string AcquirerIcaNumber { get; set; }
    public string Token { get; set; }
    public CardType CardType { get; set; }
    public MasterpassTransactionType TransactionType { get; set; }
    public string CardNumber { get; set; }
    public string CardHolderName { get; set; }
}

public class MasterpassTopupProcessServiceRequest : MasterpassTopupProcessRequest, IHasUserId
{
    public Guid UserId { get; set; }
}
