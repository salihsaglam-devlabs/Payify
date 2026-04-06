using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Requests;

public class GetCompanyIbanListRequest : SearchQueryParams
{
    public string Iban { get; set; }
    public int? BankCode { get; set; }
    public string BankName { get; set; }

    public RecordStatus? RecordStatus { get; set; }
}
