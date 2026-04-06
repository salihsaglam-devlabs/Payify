using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Requests;

public class GetOperationalTransferListRequest : SearchQueryParams
{
    public int? SenderBankCode { get; set; }
    public int? ReceiverBankCode { get; set; }
    public DateTime? TransactionDateStart { get; set; }
    public DateTime? TransactionDateEnd { get; set; }
}
