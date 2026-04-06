using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Enums;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;

public class BulkIntegrationModeUpdateRequest
{
    public Guid[] ParentMerchantIdList { get; set; }
    public Guid? MainSubMerchantId { get; set; }
    public int? CityCode { get; set; }
    public IntegrationMode? IntegrationMode { get; set; }
    public MerchantType? MerchantType { get; set; }
    public MerchantStatus? MerchantStatus { get; set; }
    public DateTime? CreateDateStart { get; set; }
    public DateTime? CreateDateEnd { get; set; }
    public bool? IsApiMode { get; set; }
    public bool? IsHppMode { get; set; }
    public bool? IsManuelPaymentPageMode { get; set; }
    public bool? IsLinkPaymentPageMode { get; set; }
    public bool? IsOnUsMode { get; set; }
}
