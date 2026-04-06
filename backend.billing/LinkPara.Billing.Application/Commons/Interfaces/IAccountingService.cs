using LinkPara.Billing.Domain.Entities;

namespace LinkPara.Billing.Application.Commons.Interfaces;

public interface IAccountingService
{
    Task PostAccountingPaymentAsync(Transaction transaction, Guid emoneyTransactionId);
    Task CancelAccountingPaymentAsync(Transaction transaction);
}