using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;

public class GetAllMerchantInstallmentTransactionRequest : SearchQueryParams
{
    public Guid MerchantTransactionId { get; set; }
}
