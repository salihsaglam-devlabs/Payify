using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Enums;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;

public class GetFilterTransactionTimeoutRequest : SearchQueryParams
{
    public Guid? MerchantId { get; set; }
    public int? AcquireBankCode { get; set; }
    public DateTime? TransactionDateStart { get; set; }
    public DateTime? TransactionDateEnd { get; set; }
    public TimeoutTransactionType? TransactionType { get; set; }
    public string OriginalOrderId { get; set; }
    public string ConversationId { get; set; }
    public TimeoutTransactionStatus? TimeoutTransactionStatus { get; set; }
    public string CardFirstNumbers { get; set; }
    public string CardLastNumbers { get; set; }
}