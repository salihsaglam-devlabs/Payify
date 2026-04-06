using LinkPara.PF.Domain.Entities;

namespace LinkPara.PF.Application.Commons.Interfaces
{
    public interface IAccountingService
    {
        public Task CreateCustomerAsync(Merchant merchant, Customer customer, ContactPerson contactPerson);
        Task<string> GetCustomerCodeInitialAsync();
    }
}