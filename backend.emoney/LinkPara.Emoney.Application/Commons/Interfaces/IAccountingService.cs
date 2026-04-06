using LinkPara.Emoney.Domain.Entities;
using LinkPara.HttpProviders.CustomerManagement.Models;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Accounting;

namespace LinkPara.Emoney.Application.Commons.Interfaces;

public interface IAccountingService
{
    public Task CreateCustomerAsync(Account account, Wallet wallet, CustomerDto customer);
    public Task SavePaymentAsync(AccountingPayment payment);
    public Task UpdateCustomerAsync(CustomerDto customer);
}
