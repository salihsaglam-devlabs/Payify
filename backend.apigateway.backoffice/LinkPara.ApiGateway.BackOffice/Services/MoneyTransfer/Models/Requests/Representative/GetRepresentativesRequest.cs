using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Requests.Representative;

public class GetRepresentativesRequest : SearchQueryParams
{
    public string Title { get; set; }
    public string Code { get; set; }
    public DateTime? MinCreateDate { get; set; }
    public DateTime? MaxCreateDate { get; set; }
    public RecordStatus? RecordStatus { get; set; }
}