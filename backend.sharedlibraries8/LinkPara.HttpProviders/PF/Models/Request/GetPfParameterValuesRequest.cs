using LinkPara.SharedModels.Pagination;

namespace LinkPara.HttpProviders.PF.Models.Request;

public class GetPfParameterValuesRequest : SearchQueryParams
{
    public string Language { get; set; }
    public Guid MerchantId { get; set; }
} 