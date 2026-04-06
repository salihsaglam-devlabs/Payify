using LinkPara.ApiGateway.BackOffice.Commons.Extensions;
using LinkPara.ApiGateway.BackOffice.Commons.Helpers;
using LinkPara.ApiGateway.BackOffice.Services.Epin.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Epin.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Http;

namespace LinkPara.ApiGateway.BackOffice.Services.Epin.HttpClients;

public class OrderHttpClient : HttpClientBase, IOrderHttpClient
{
    public OrderHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor) : base(client, httpContextAccessor)
    {
    }

    public async Task CancelOrderAsync(Guid orderId)
    {
        var response = await DeleteAsync($"v1/Orders/cancel/{orderId}");
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
    }

    public async Task<PaginatedList<OrderDto>> GetOrdersFilterAsync(GetOrdersFilterRequest request)
    {
        var url = CreateUrlWithParams($"v1/Orders", request, true);
        var response = await GetAsync(url);
        var results = await response.Content.ReadFromJsonAsync<PaginatedList<OrderDto>>();
        
        if (!CanSeeSensitiveData())
        {
            results.Items.ForEach(s =>
            {
                s.UserFullName = SensitiveDataHelper.MaskSensitiveData("FullName", s.UserFullName);
                s.Email = SensitiveDataHelper.MaskSensitiveData("Email", s.Email);
            });    
        }

        return results;
    }

    public async Task<OrderSummaryDto> GetOrderSummaryAsync(Guid id)
    {
        var response = await GetAsync($"v1/Orders/summary/{id}");
        var result = await response.Content.ReadFromJsonAsync<OrderSummaryDto>();
        if (!CanSeeSensitiveData())
        {
            result.PhoneNumber = SensitiveDataHelper.MaskSensitiveData("PhoneNumber", result.PhoneNumber);
        }

        return result;
    }
}
