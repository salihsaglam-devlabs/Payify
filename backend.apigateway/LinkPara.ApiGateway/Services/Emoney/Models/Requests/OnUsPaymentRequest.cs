using LinkPara.ApiGateway.Services.Emoney.Models.Enums;

namespace LinkPara.ApiGateway.Services.Emoney.Models.Requests;

public class OnUsPaymentRequest
{
    public Guid Id { get; set; }
    public string MerchantName { get; set; }
    public string MerchantNumber { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; }
    public OnUsPaymentStatus Status { get; set; }
    public string PhoneCode { get; set; }
    public string PhoneNumber { get; set; }
    public string WalletNumber { get; set; }
    public Guid WalletId { get; set; }
    public Guid TransactionId { get; set; }
    public string ErrorCode { get; set; }
    public string ErrorMessage { get; set; }
    public string CancelDescription { get; set; }
    public string ConversationId { get; set; }
    public string OrderId { get; set; }
    public DateTime ExpireDate { get; set; }
    public DateTime RequestDate { get; set; }
    public string MerchantTransactionId { get; set; }
}