using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.Boa.Services.Pf.Models.Requests;

public class GetFilterMccRequest : SearchQueryParams
{
    public string Name { get; set; }
    public DateTime? CreateDateStart { get; set; }
    public DateTime? CreateDateEnd { get; set; }
    public RecordStatus? RecordStatus { get; set; }
}
