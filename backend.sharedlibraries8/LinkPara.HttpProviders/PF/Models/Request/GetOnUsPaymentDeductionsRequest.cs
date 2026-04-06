using LinkPara.SharedModels.Pagination;

namespace LinkPara.HttpProviders.PF.Models.Request;

public class GetOnUsPaymentDeductionsRequest : SearchQueryParams
{
    public Guid MerchantTransactionId { get; set; }
}