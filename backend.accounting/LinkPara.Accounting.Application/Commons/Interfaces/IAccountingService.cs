using LinkPara.Accounting.Application.Commons.Models;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Accounting;

namespace LinkPara.Accounting.Application.Commons.Interfaces;

public interface IAccountingService
{
    Task PostPaymentAsync(AccountingPayment paymentRequest);
    Task CreateCustomerAsync(AccountingCustomer customerRequest);
    Task CancelPaymentAsync(Guid clientReferenceId);
    Task<Guid> ProcessInvoiceAsync(ProcessInvoiceRequest processInvoiceRequest);
    Task UpdateAccountingCustomerAsync(UpdateAccountingCustomer updateAccountingCustomer);
}
