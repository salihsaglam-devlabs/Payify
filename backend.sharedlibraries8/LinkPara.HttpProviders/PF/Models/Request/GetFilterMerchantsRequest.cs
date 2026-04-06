using LinkPara.HttpProviders.PF.Models.Enums;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.HttpProviders.PF.Models.Request;

public class GetFilterMerchantsRequest : SearchQueryParams
{
    public Guid? MerchantId { get; set; }
    public string MerchantNumber { get; set; }
    public string MerchantName { get; set; }
    public MerchantType? MerchantType { get; set; }
    public Guid? ParentMerchantId { get; set; }
    public DateTime? CreateDateStart { get; set; }
    public DateTime? CreateDateEnd { get; set; }
    public ApplicationChannel? ApplicationChannel { get; set; }
    public CompanyType? CompanyType { get; set; }
    public MerchantStatus? MerchantStatus { get; set; }
    public IntegrationMode? IntegrationMode { get; set; }
    public PostingPaymentChannel? PostingPaymentChannel { get; set; }
    public int? CountryCode { get; set; }
    public int? CityCode { get; set; }
    public bool? IsBlockage { get; set; }
    public string PricingProfileNumber { get; set; }
    public bool? IsReturnAllowed { get; set; }
    public bool? IsReverseAllowed { get; set; }
    public bool? IsInstallmentAllowed { get; set; }
    public bool? IsPreAuthAllowed { get; set; }
    public bool? IsInternationalCardAllowed { get; set; }
    public string MccCode { get; set; }
}