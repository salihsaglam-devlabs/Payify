using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;

public class GetAllSubMerchantRequest : SearchQueryParams
{
    public Guid? MerchantId { get; set; }
    public int? CityCode { get; set; }
    public DateTime? CreateDateStart { get; set; }
    public DateTime? CreateDateEnd { get; set; }
    public RecordStatus? RecordStatus { get; set; }
    public string Number { get; set; }
    public bool? IsManuelPaymentPageAllowed { get; set; }
    public bool? IsLinkPaymentPageAllowed { get; set; }
    public bool? IsOnUsPaymentPageAllowed { get; set; }
    public bool? PaymentReverseAllowed { get; set; }
    public bool? PaymentReturnAllowed { get; set; }
}
