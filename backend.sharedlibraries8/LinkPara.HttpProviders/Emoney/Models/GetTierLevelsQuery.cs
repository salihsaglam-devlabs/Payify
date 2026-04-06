using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.HttpProviders.Emoney.Models
{
    public class GetTierLevelsQuery : SearchQueryParams
    {
        public string CurrencyCode { get; set; }
        public RecordStatus? RecordStatus { get; set; }
        public bool IncludeCustoms { get; set; }
    }
}
