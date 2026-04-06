using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Enums;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.Merchant.Services.Pf.Models.Requests;

public class GetAllPostingBalanceRequest : SearchQueryParams
{
    public Guid? MerchantId { get; set; }
    public BlockageStatus? BlockageStatus { get; set; }
    public DateTime? PaymentDateStart { get; set; }
    public DateTime? PaymentDateEnd { get; set; }
    public DateTime? TransactionDateStart { get; set; }
    public DateTime? TransactionDateEnd { get; set; }
    public PostingMoneyTransferStatus?[] MoneyTransferStatus { get; set; }
    public PostingBalanceType?[] PostingBalanceType { get; set; }
    public bool? IsPayment { get; set; }
    public PostingPaymentChannel? PostingPaymentChannel { get; set; }
}
