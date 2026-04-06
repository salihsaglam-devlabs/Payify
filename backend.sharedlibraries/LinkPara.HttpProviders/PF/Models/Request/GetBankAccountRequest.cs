using LinkPara.SharedModels.Pagination;

namespace LinkPara.HttpProviders.PF.Models.Request;

public class GetBankAccountRequest : SearchQueryParams
{
    public Guid MerchantId { get; set; }
}
