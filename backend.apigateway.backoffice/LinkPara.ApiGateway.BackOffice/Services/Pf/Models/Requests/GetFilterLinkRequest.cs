using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Enums;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;

public class GetFilterLinkRequest : SearchQueryParams
{
    public Guid MerchantId { get; set; }
    public Guid? SubMerchantId { get; set; }
    public LinkSearchType LinkSearchType { get; set; }
    public LinkInfoSearchRequest LinkInfoSearchRequest { get; set; }
    public LinkTransactionSearchRequest LinkTransactionSearchRequest { get; set; }
    public LinkCustomerSearchRequest LinkCustomerSearchRequest { get; set; }
}
