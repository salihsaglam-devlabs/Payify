using LinkPara.HttpProviders.BTrans.Models.Enums;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.HttpProviders.BTrans.Models;

public class GetDocumentsRequest : SearchQueryParams
{
    public DateTime? CreateDateStart { get; set; }
    public DateTime? CreateDateEnd { get; set; }
    public DocumentStatus? DocumentStatus { get; set; }
    public string DocumentCode { get; set; }
}