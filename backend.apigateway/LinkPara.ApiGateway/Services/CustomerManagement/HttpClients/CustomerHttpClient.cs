using LinkPara.ApiGateway.Services.CustomerManagement.Models.Request;
using LinkPara.ApiGateway.Services.Identity.Models.Requests;
using LinkPara.HttpProviders.CustomerManagement.Models;
using LinkPara.SharedModels.Exceptions;
using System.Text.Json;

namespace LinkPara.ApiGateway.Services.CustomerManagement.HttpClients
{
    public class CustomerHttpClient : HttpClientBase, ICustomerHttpClient
    {
        public CustomerHttpClient(HttpClient client,
                 IHttpContextAccessor httpContextAccessor) : base(client, httpContextAccessor)
        {

        }

        public async Task UpdateCustomerAddressAsync(UpdateCustomerAddressRequest request)
        {
            var response = await PutAsJsonAsync($"v1/Customers/UpdateCustomerAddress", request);
            if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
        }

        public async Task AddCustomerAddressAsync(AddCustomerAddressRequest request)
        {
            var response = await PostAsJsonAsync($"v1/Customers/AddCustomerAddress", request);
            if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
        }

        public async Task<CustomerDto> GetCustomerAsync(Guid customerId)
        {
            var response = await GetAsync($"v1/Customers/{customerId}");
            var responseString = await response.Content.ReadAsStringAsync();

            var customer = JsonSerializer.Deserialize<CustomerDto>(responseString, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return customer ?? throw new NotFoundException();
        }
    }
}
