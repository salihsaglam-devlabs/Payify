using LinkPara.PF.Domain.Enums;

namespace LinkPara.PF.Application.Commons.Models.MerchantTransactions;

public class MerchantTransactionRequest
{
    public Guid MerchantId { get; set; }
    public DateTime StartDate { get; set; } 
    public DateTime EndDate { get; set; }
    public TransactionType TransactionType { get; set; }
}