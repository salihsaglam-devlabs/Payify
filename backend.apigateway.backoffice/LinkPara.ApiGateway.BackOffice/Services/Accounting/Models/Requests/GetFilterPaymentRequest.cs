using LinkPara.ApiGateway.BackOffice.Services.Accounting.Models.Enums;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Accounting.Models.Requests;

public class GetFilterPaymentRequest : SearchQueryParams
{
    public DateTime? TransactionDateStart { get; set; }
    public DateTime? TransactionDateEnd { get; set; }
    public bool? IsSuccess { get; set; }
    public string Source { get; set; }
    public string Destination { get; set; }
    public AccountingOperationType? OperationType { get; set; }
}
