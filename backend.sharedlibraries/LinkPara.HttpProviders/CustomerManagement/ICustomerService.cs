using LinkPara.HttpProviders.CustomerManagement.Models;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.HttpProviders.CustomerManagement
{
    public interface ICustomerService
    {
        Task<CreateCustomerResponse> CreateCustomerAsync(CreateCustomerRequest request);
        Task<PaginatedList<CustomerDto>> GetAllCustomersAsync(GetCustomersRequest request);
        Task<CustomerDto> GetCustomerAsync(Guid userId);
        Task<CustomerDto> GetCustomerByCustomerNumberAsync(int customerNumber);
        Task AddCustomerAddressAsync(AddAddressRequest request);
    }
}
