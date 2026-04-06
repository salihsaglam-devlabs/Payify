using LinkPara.ApiGateway.BackOffice.Services.BTrans.Models.Enum;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.BTrans.Models.Request;

public class GetDocumentsRequest : SearchQueryParams
{
    public DateTime? CreateDateStart { get; set; }
    public DateTime? CreateDateEnd { get; set; }
    public DocumentStatus? DocumentStatus { get; set; }
    public string DocumentCode { get; set; }
}