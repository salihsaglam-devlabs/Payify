using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Requests;

public class GetOperationalTransferBalanceListRequest : SearchQueryParams
{
    public int? BankCode { get; set; }
}
