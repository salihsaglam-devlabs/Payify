using LinkPara.PF.Domain.Enums;

namespace LinkPara.PF.Application.Commons.Models.Payments;

public class ProvisionModel
{
    public decimal Amount { get; set; }
    public TransactionStatus TransactionStatus { get; set; }
    public TransactionType TransactionType { get; set; }
    public string TransactionDate { get; set; }
}