using LinkPara.PF.Domain.Enums;

namespace LinkPara.PF.Application.Commons.Models.MerchantTransactions;

public class MerchantTransactionStatusModel
{
    public TransactionStatus TransactionStatus { get; set; }
    public int Count { get; set; }
    public double Percent { get; set; }
}
