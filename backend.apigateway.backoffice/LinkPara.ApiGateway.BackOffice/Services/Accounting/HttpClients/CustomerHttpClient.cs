using LinkPara.ApiGateway.BackOffice.Commons.Helpers;
using LinkPara.ApiGateway.BackOffice.Services.Accounting.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Accounting.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Services.Accounting.HttpClients;

public class CustomerHttpClient : HttpClientBase, ICustomerHttpClient
{
    public CustomerHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor) : base(client, httpContextAccessor)
    {
    }

    public async Task<ActionResult<AccountingCustomerDto>> GetByIdAsync(Guid id)
    {
        var response = await GetAsync($"v1/Customers/{id}");

        var customer = await response.Content.ReadFromJsonAsync<AccountingCustomerDto>();

        if (!CanSeeSensitiveData())
        {
            customer.FirstName = SensitiveDataHelper.MaskSensitiveData("Name", customer.FirstName);
            customer.LastName = SensitiveDataHelper.MaskSensitiveData("Name", customer.LastName);
            customer.Email = SensitiveDataHelper.MaskSensitiveData("Email", customer.Email);
            customer.PhoneNumber = SensitiveDataHelper.MaskSensitiveData("PhoneNumber", customer.PhoneNumber);
            customer.IdentityNumber = SensitiveDataHelper.MaskSensitiveData("IdentityNumber", customer.IdentityNumber);
        }

        return customer;
    }

    public async Task<ActionResult<PaginatedList<AccountingCustomerDto>>> GetListCustomersAsync(GetFilterCustomerRequest request)
    {
        var url = CreateUrlWithParams($"v1/Customers", request, true);
        var response = await GetAsync(url);
        var customers = await response.Content.ReadFromJsonAsync<PaginatedList<AccountingCustomerDto>>();

        if (!CanSeeSensitiveData())
        {
            customers.Items.ForEach(s =>
            {
                s.FirstName = SensitiveDataHelper.MaskSensitiveData("Name", s.FirstName);
                s.LastName = SensitiveDataHelper.MaskSensitiveData("Name", s.LastName);
                s.Email = SensitiveDataHelper.MaskSensitiveData("Email", s.Email);
                s.PhoneNumber = SensitiveDataHelper.MaskSensitiveData("PhoneNumber", s.PhoneNumber);
                s.IdentityNumber = SensitiveDataHelper.MaskSensitiveData("IdentityNumber", s.IdentityNumber);
            });
        }

        return customers ?? throw new InvalidOperationException();
    }

    public async Task SaveCustomerAsync(SaveCustomerRequest request)
    {
        await PostAsJsonAsync("v1/Customers", request);
    }
}
