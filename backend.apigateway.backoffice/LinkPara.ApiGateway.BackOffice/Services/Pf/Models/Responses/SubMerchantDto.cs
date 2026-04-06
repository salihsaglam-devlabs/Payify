using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;

public class SubMerchantDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Number { get; set; }
    public MerchantType MerchantType { get; set; }
    public Guid MerchantId { get; set; }
    public string MerchantName { get; set; }
    public string MerchantNumber { get; set; }
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
    public bool InternationalCardAllowed { get; set; }
    public string RejectReason { get; set; }
    public DateTime CreateDate { get; set; }
    public RecordStatus RecordStatus { get; set; }
    public List<SubMerchantDocumentDto> SubMerchantDocuments { get; set; }
    public List<SubMerchantLimitDto> SubMerchantLimits { get; set; }
}
