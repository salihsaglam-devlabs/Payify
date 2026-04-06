using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Enums;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Requests;

public class GetBankResponseCodeListRequest : SearchQueryParams
{
    public ResponseCodeAction? Action { get; set; }
    public RecordStatus? RecordStatus { get; set; }
    public int BankCode { get; set; }
    public string BankName { get; set; }
}
