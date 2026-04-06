using LinkPara.ApiGateway.BackOffice.Services.CustomerManagement.Models.Request;
using LinkPara.ApiGateway.BackOffice.Services.CustomerManagement.Models.Response;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.CustomerManagement.HttpClients
{
    public interface ICustomerManagementClient
    {
        Task<PaginatedList<CustomerResponse>> GetAllCustomersAsync(GetCustomersRequest request);
        Task UpdateCustomerAddressAsync(UpdateCustomerAddressRequest request);
    }
}
