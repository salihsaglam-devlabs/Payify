using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Enums;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.Merchant.Services.Pf.Models.Requests;

public class GetAllTransactionRequest : SearchQueryParams
{
    public Guid? PostingBalanceId { get; set; }
    public Guid? MerchantId { get; set; }
    public DateTime? PostingDate { get; set; }
    public BlockageStatus? BlockageStatus { get; set; }
    public TimeoutTransactionType?[] TransactionType { get; set; }
}
