using LinkPara.ApiGateway.Services.CustomerManagement.Models.Request;
using LinkPara.HttpProviders.CustomerManagement.Models;

namespace LinkPara.ApiGateway.Services.CustomerManagement.HttpClients
{
    public interface ICustomerHttpClient
    {
        Task UpdateCustomerAddressAsync(UpdateCustomerAddressRequest request);
        Task AddCustomerAddressAsync(AddCustomerAddressRequest request);
        Task<CustomerDto> GetCustomerAsync(Guid customerId);
    }
}
