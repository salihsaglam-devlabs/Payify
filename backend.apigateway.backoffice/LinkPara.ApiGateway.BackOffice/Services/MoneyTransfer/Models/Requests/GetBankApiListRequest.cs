using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Requests;

public class GetBankApiListRequest : SearchQueryParams
{
    public RecordStatus? RecordStatus { get; set; }
    public int? BankCode { get; set; }
}
