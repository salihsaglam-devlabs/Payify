using LinkPara.HttpProviders.CustomerManagement.Models;
using LinkPara.HttpProviders.Utility;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Json;
using System.Text.Json;


namespace LinkPara.HttpProviders.CustomerManagement
{
    public class CustomerService : HttpClientBase, ICustomerService
    {
        
        public CustomerService(HttpClient client, IHttpContextAccessor httpContextAccessor)
        : base(client, httpContextAccessor)
        {

        }

        public async Task<CreateCustomerResponse> CreateCustomerAsync(CreateCustomerRequest request)
        {
            var response = await PostAsJsonAsync("v1/Customers", request);

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException();
            }

            var customerId = await response.Content.ReadFromJsonAsync<CreateCustomerResponse>();

            return customerId ?? throw new InvalidOperationException();
        }

        public async Task<PaginatedList<CustomerDto>> GetAllCustomersAsync(GetCustomersRequest request)
        {
            var url = GetQueryString.CreateUrlWithSearchQueryParams($"v1/Customers", request, true);

            var response = await GetAsync(url);

            var customers = await response.Content.ReadFromJsonAsync<PaginatedList<CustomerDto>>();

            return customers ?? throw new InvalidOperationException();
        }

        public async Task<CustomerDto> GetCustomerAsync(Guid id)
        {
            var response = await GetAsync($"v1/Customers/{id}");

            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();

                var customer = JsonSerializer.Deserialize<CustomerDto>(responseString, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return customer;
            }
            throw new InvalidOperationException();
        }
        
        public async Task<CustomerDto> GetCustomerByCustomerNumberAsync(int customerNumber)
        {
            var response = await GetAsync($"v1/Customers/{customerNumber}/customer");

            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();

                var customer = JsonSerializer.Deserialize<CustomerDto>(responseString, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return customer;
            }
            throw new InvalidOperationException();
        }

        public async Task AddCustomerAddressAsync(AddAddressRequest request)
        {
            var response = await PostAsJsonAsync("v1/Customers/AddCustomerAddress", request);

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException();
            }
        }
    }
}
