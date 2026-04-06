using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.PF.Domain.Entities;

public class SubMerchant : AuditEntity, ITrackChange
{
    public string Name { get; set; }
    public string Number { get; set; }
    public MerchantType MerchantType { get; set; }
    public Guid MerchantId { get; set; }
    public int City { get; set; }
    public string CityName { get; set; }
    public bool IsManuelPaymentPageAllowed { get; set; }
    public bool IsLinkPaymentPageAllowed { get; set; }
    public bool IsOnUsPaymentPageAllowed { get; set; }
    public bool PreAuthorizationAllowed { get; set; }
    public bool PaymentReverseAllowed { get; set; }
    public bool PaymentReturnAllowed { get; set; }
    public bool InstallmentAllowed { get; set; }
    public bool Is3dRequired { get; set; }
    public bool IsExcessReturnAllowed { get; set; }
    public string RejectReason { get; set; }
    public string ParameterValue { get; set; }
    public bool InternationalCardAllowed { get; set; }
    public List<SubMerchantDocument> SubMerchantDocuments { get; set; }
    public List<SubMerchantLimit> SubMerchantLimits { get; set; }
    public List<SubMerchantUser> SubMerchantUsers { get; set; }
}
