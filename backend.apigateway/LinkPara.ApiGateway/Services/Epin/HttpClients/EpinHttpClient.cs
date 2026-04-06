using LinkPara.ApiGateway.Commons.Extensions;
using LinkPara.ApiGateway.Commons.Helpers;
using LinkPara.ApiGateway.Services.Emoney.HttpClients;
using LinkPara.ApiGateway.Services.Emoney.Models.Requests;
using LinkPara.ApiGateway.Services.Epin.Models.Requests;
using LinkPara.ApiGateway.Services.Epin.Models.Responses;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace LinkPara.ApiGateway.Services.Epin.HttpClients;

public class EpinHttpClient : HttpClientBase, IEpinHttpClient
{
    private readonly IServiceRequestConverter _serviceRequestConverter;
    private readonly IWalletHttpClient _walletHttpClient;

    public EpinHttpClient(HttpClient client
        , IHttpContextAccessor httpContextAccessor
        , IServiceRequestConverter serviceRequestConverter
        , IWalletHttpClient walletHttpClient)
    : base(client, httpContextAccessor)
    {
        _serviceRequestConverter = serviceRequestConverter;
        _walletHttpClient = walletHttpClient;
    }

    public async Task<CreateOrderResponse> CreateOrderAsync(CreateOrderRequest request, Guid userId)
    {
        var userWallets = await _walletHttpClient.GetUserWalletsAsync(new GetUserWalletsFilterRequest { });

        if (userWallets.Any(u => u.WalletNumber.Equals(request.WalletNumber)))
        {
            var response = await PostAsJsonAsync($"v1/Orders", request);
            var result= await response.Content.ReadFromJsonAsync<CreateOrderResponse>();
            return result ?? throw new InvalidOperationException();
        }
        throw new ForbiddenAccessException();
    }

    public async Task<ActionResult<PaginatedList<BrandDto>>> GetFilterBrandsAsync(GetFilterBrandsRequest request)
    {
        var queryString = request.GetQueryString();
        var response = await GetAsync($"v1/Brands{queryString}");

        return await response.Content.ReadFromJsonAsync<PaginatedList<BrandDto>>();
    }

    public async Task<ActionResult<List<ProductDto>>> GetFilterProductsAsync(GetFilterProductsRequest request)
    {
        var queryString = request.GetQueryString();
        var response = await GetAsync($"v1/Products{queryString}");

        return await response.Content.ReadFromJsonAsync<List<ProductDto>>();
    }

    public async Task<ActionResult<PaginatedList<PublisherDto>>> GetFilterPublishersAsync(GetFilterPublishersRequest request)
    {
        var queryString = request.GetQueryString();
        var response = await GetAsync($"v1/Publishers{queryString}");

        return await response.Content.ReadFromJsonAsync<PaginatedList<PublisherDto>>();
    }

    public async Task<PaginatedList<UserOrderDto>> GetUserOrdersFilterAsync(GetUserOrdersFilterRequest request)
    {
        var clientRequest = _serviceRequestConverter.Convert<GetUserOrdersFilterRequest, GetUserOrdersFilterServiceRequest>(request);
        var queryString = clientRequest.GetQueryString();

        var response = await GetAsync($"v1/Orders/{clientRequest.UserId}" + queryString);
        var responseString = await response.Content.ReadAsStringAsync();
        var orderList = JsonSerializer.Deserialize<PaginatedList<UserOrderDto>>(responseString, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return orderList ?? throw new InvalidOperationException();
    }
}
