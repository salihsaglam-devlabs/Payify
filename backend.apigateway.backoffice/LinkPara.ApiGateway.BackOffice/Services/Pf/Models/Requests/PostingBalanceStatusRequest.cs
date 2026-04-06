using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;

public class PostingBalanceStatusRequest : SearchQueryParams
{
    public Guid? MerchantId { get; set; }
    public DateTime? TransactionDateStart { get; set; }
    public DateTime? TransactionDateEnd { get; set; }
}
