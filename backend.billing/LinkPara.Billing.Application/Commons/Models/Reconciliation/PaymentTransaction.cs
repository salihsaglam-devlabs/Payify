using LinkPara.Billing.Domain.Enums;

namespace LinkPara.Billing.Application.Commons.Models.Reconciliation;

public class PaymentTransaction : Payment
{
    public TransactionStatus TransactionStatus { get; set; }
}