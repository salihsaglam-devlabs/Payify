using LinkPara.ApiGateway.BackOffice.Commons.Helpers;
using LinkPara.ApiGateway.BackOffice.Services.CustomerManagement.Models.Request;
using LinkPara.ApiGateway.BackOffice.Services.CustomerManagement.Models.Response;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.CustomerManagement.HttpClients
{
    public class CustomerManagementClient : HttpClientBase, ICustomerManagementClient
    {
        public CustomerManagementClient(HttpClient client,
                 IHttpContextAccessor httpContextAccessor) : base(client, httpContextAccessor)
        {

        }
        public async Task<PaginatedList<CustomerResponse>> GetAllCustomersAsync(GetCustomersRequest request)
        {
            var url = CreateUrlWithParams($"v1/Customers", request, true);
            var response = await GetAsync(url);
            var parameters = await response.Content.ReadFromJsonAsync<PaginatedList<CustomerResponse>>();
            if (!CanSeeSensitiveData())
            {
                parameters.Items.ForEach(customer =>
                {
                    customer.FirstName = SensitiveDataHelper.MaskSensitiveData("Name", customer.FirstName);
                    customer.LastName = SensitiveDataHelper.MaskSensitiveData("Name", customer.LastName);
                    customer.IdentityNumber = SensitiveDataHelper.MaskSensitiveData("IdentityNumber", customer.IdentityNumber);
                    customer.TaxNumber = SensitiveDataHelper.MaskSensitiveData("TaxNumber", customer.TaxNumber);
                    customer.CustomerEmails?.ForEach(email =>
                    {
                        email.Email = SensitiveDataHelper.MaskSensitiveData("Email", email.Email);
                    });
                    customer.CustomerAddresses?.ForEach(address =>
                    {
                        address.Address = SensitiveDataHelper.MaskSensitiveData("Address", address.Address);
                    });
                    customer.CustomerPhones?.ForEach(phone =>
                    {
                        phone.PhoneNumber = SensitiveDataHelper.MaskSensitiveData("PhoneNumber", phone.PhoneNumber);
                    });
                });
            }
            return parameters ?? throw new InvalidOperationException();
        }

        public async Task UpdateCustomerAddressAsync(UpdateCustomerAddressRequest request)
        {
            var response = await PutAsJsonAsync($"v1/Customers/UpdateCustomerAddress", request);
            if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
        }
    }
}
