using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.PF.Domain.Entities;

public class HostedPayment : AuditEntity, ITrackChange
{
    public string TrackingId { get; set; }
    public ChannelStatus HppStatus { get; set; }
    public ChannelPaymentStatus HppPaymentStatus { get; set; }
    public WebhookStatus WebhookStatus { get; set; }
    public string OrderId { get; set; }
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
    public bool EnableInstallments { get; set; }
    public List<HostedPaymentInstallment> Installments { get; set; }
    public HppPageViewType PageViewType { get; set; }
}