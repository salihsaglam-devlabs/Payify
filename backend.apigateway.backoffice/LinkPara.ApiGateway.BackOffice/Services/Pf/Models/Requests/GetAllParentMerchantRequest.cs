using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Enums;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;

public class GetAllParentMerchantRequest : SearchQueryParams
{
    public Guid[] ParentMerchantIdList { get; set; }
    public Guid? MainSubMerchantId { get; set; }
    public int? CityCode { get; set; }
    public IntegrationMode? IntegrationMode { get; set; }
    public MerchantType? MerchantType { get; set; }
    public MerchantStatus? MerchantStatus { get; set; }
    public DateTime? CreateDateStart { get; set; }
    public DateTime? CreateDateEnd { get; set; }
}
