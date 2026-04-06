using LinkPara.ApiGateway.CorporateWallet.Commons.Helpers;
using LinkPara.ApiGateway.CorporateWallet.Services.Emoney.Models.Requests;
using LinkPara.ApiGateway.CorporateWallet.Services.Emoney.Models.Responses;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Pagination;
using System.Security.Claims;

namespace LinkPara.ApiGateway.CorporateWallet.Services.Emoney.HttpClients;

public class TransferOrderHttpClient : HttpClientBase, ITransferOrderHttpClient
{
    private readonly IServiceRequestConverter _serviceRequestConverter;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private string UserId;

    public TransferOrderHttpClient(HttpClient client, IServiceRequestConverter serviceRequestConverter, IHttpContextAccessor httpContextAccessor)
        : base(client, httpContextAccessor)
    {
        _serviceRequestConverter = serviceRequestConverter;
        _httpContextAccessor = httpContextAccessor;
        UserId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
    }

    public async Task<PaginatedList<TransferOrderDto>> GetTransferOrdersByUserIdAsync(GetTransferOrdersRequest query)
    {
        var url = CreateUrlWithParams($"v1/TransferOrders", query, true);
        var response = await GetAsync(url);
        var transferOrders = await response.Content.ReadFromJsonAsync<PaginatedList<TransferOrderDto>>();
        if (transferOrders.Items.Count > 0 && transferOrders.Items.Any(q => q.UserId.ToString() != UserId))
        {
            throw new ForbiddenAccessException();
        }
        return transferOrders ?? throw new InvalidOperationException();
    }

    public async Task<TransferOrderDto> GetTransferOrderByIdAsync(Guid transferOrderId)
    {
        var response = await GetAsync($"v1/TransferOrders/{transferOrderId}");
        var transferOrder = await response.Content.ReadFromJsonAsync<TransferOrderDto>();

        if (transferOrder.UserId.ToString() != UserId)
        {
            throw new ForbiddenAccessException();
        }

        return transferOrder ?? throw new InvalidOperationException();
    }

    public async Task CreateTransferOrderAsync(CreateTransferOrderRequest request)
    {
        var clientRequest = _serviceRequestConverter.Convert<CreateTransferOrderRequest, CreateTransferOrderServiceRequest>(request);

        var response = await PostAsJsonAsync($"v1/TransferOrders", clientRequest);
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
    }

    public async Task UpdateTransferOrderAsync(Guid transferOrderId, UpdateTransferOrderRequest request)
    {
        var clientRequest = _serviceRequestConverter.Convert<UpdateTransferOrderRequest, UpdateTransferOrderServiceRequest>(request);
        clientRequest.TransferOrderId = transferOrderId;

        var response = await PutAsJsonAsync($"v1/TransferOrders/{clientRequest.TransferOrderId}", clientRequest);
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
    }

    public async Task DeleteTransferOrderAsync(Guid transferOrderId)
    {
        var response = await DeleteAsync($"v1/TransferOrders/{transferOrderId}");
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
    }
}