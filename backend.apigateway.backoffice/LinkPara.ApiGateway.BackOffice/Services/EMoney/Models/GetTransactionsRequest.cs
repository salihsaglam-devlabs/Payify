using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Enums;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.BackOffice.Services.EMoney.Models;
public class GetTransactionsRequest : SearchQueryParams
{
    public string TransactionTypes { get; set; }
    public PaymentMethod? PaymentMethod { get; set; }
    public TransactionStatus? TransactionStatus { get; set; }
    public RecordStatus? RecordStatus { get; set; }
    public string CurrencyCode { get; set; }
    public string Tag { get; set; }
    public string ExternalReferenceId { get; set; }
    public string Description { get; set; }
    public Guid? TransactionId { get; set; }
    public Guid? UserId { get; set; }
    public Guid? WalletId { get; set; }
    public Guid? RelatedTransactionId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int? ReceiverBankCode { get; set; }
    public string ReceiverName { get; set; }
    public int? SenderBankCode { get; set; }
    public string SenderName { get; set; }
    public TransactionDirection? TransactionDirection { get; set; }
    public string WalletNumber { get; set; }
}
