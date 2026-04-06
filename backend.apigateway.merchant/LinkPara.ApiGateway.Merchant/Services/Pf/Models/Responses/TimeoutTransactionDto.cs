using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Enums;

namespace LinkPara.ApiGateway.Merchant.Services.Pf.Models.Responses;

public class TimeoutTransactionDto
{
    public Guid Id { get; set; }
    public TimeoutTransactionStatus TimeoutTransactionStatus { get; set; }
    public TimeoutTransactionType TransactionType { get; set; }
    public string CardNumber { get; set; }
    public string OriginalOrderId { get; set; }
    public string ConversationId { get; set; }
    public string OrderId { get; set; }
    public decimal Amount { get; set; }
    public string SubMerchantCode { get; set; }
    public DateTime TransactionDate { get; set; }
    public Guid MerchantId { get; set; }
    public TransactionMerchantResponse Merchant { get; set; }
    public int AcquireBankCode { get; set; }
    public BankDto AcquireBank { get; set; }
    public Guid VposId { get; set; }
    public Guid MerchantTransactionId { get; set; }
    public Guid BankTransactionId { get; set; }
    public int Currency { get; set; }
    public string LanguageCode { get; set; }
    public int RetryCount { get; set; }
    public DateTime? NextTryTime { get; set; }
    public string PosErrorCode { get; set; }
    public string PosErrorMessage { get; set; }
    public string ErrorCode { get; set; }
    public string ErrorMessage { get; set; }
    public string ResponseCode { get; set; }
    public string ResponseMessage { get; set; }
    public string Description { get; set; }
}
public class TransactionMerchantResponse
{
    public string Name { get; set; }
    public string Number { get; set; }
    public string WebSiteUrl { get; set; }
}
