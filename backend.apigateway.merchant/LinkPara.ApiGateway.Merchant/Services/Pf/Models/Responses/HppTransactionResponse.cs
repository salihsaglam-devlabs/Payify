using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Enums;

namespace LinkPara.ApiGateway.Merchant.Services.Pf.Models.Responses;

public class HppTransactionResponse
{
    public string TrackingId { get; set; }
    public ChannelStatus HppStatus { get; set; }
    public ChannelPaymentStatus HppPaymentStatus { get; set; }
    public WebhookStatus WebhookStatus { get; set; }
    public string OriginalOrderId { get; set; }
    public decimal Amount { get; set; }
    public int Currency { get; set; }
    public bool CommissionFromCustomer { get; set; }
    public bool Is3dRequired { get; set; }
    public string CallbackUrl { get; set; }
    public string ReturnUrl { get; set; }
    public string Name { get; set; }
    public string Surname { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string ClientIpAddress { get; set; }
    public string LanguageCode { get; set; }
    public DateTime ExpiryDate { get; set; }
    public Guid MerchantId { get; set; }
    public string MerchantName { get; set; }
    public string MerchantNumber { get; set; }
    public int WebhookRetryCount { get; set; }
    public HppPageViewType PageViewType { get; set; }
    public bool EnableInstallments { get; set; }
    public List<HostedPaymentInstallmentDto> MerchantInstallments { get; set; }
    public TransactionType TransactionType { get; set; }
    public DateTime TransactionDate { get; set; }
    public string OrderId { get; set; }
    public int InstallmentCount { get; set; }
    public string PaymentConversationId { get; set; }
    public TransactionStatus TransactionStatus { get; set; }
    public string CardNumber { get; set; }
    public CardType CardType { get; set; }
    public CardBrand CardBrand { get; set; }
    public CardNetwork CardNetwork { get; set; }
    public bool Is3ds { get; set; }
    public int IssuerBankCode { get; set; }
}