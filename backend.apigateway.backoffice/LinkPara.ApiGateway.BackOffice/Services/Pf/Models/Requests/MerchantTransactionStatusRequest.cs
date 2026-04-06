using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;

public class MerchantTransactionStatusRequest : SearchQueryParams
{
    public Guid? MerchantId { get; set; }
    public DateTime? CreateDateStart { get; set; }
    public DateTime? CreateDateEnd { get; set; }
}
