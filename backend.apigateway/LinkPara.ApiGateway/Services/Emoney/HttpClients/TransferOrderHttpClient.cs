using LinkPara.ApiGateway.Commons.Helpers;
using LinkPara.ApiGateway.Services.Emoney.Models.Requests;
using LinkPara.ApiGateway.Services.Emoney.Models.Responses;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Pagination;
using System.Security.Claims;

namespace LinkPara.ApiGateway.Services.Emoney.HttpClients;

public class TransferOrderHttpClient : HttpClientBase, ITransferOrderHttpClient
{
    private readonly IServiceRequestConverter _serviceRequestConverter;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private string UserId;
    private readonly IStringMasking _stringMasking;

    public TransferOrderHttpClient(HttpClient client, IServiceRequestConverter serviceRequestConverter, IHttpContextAccessor httpContextAccessor, IStringMasking stringMasking)
        : base(client, httpContextAccessor)
    {
        _serviceRequestConverter = serviceRequestConverter;
        _stringMasking = stringMasking;
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

        var userGuid = Guid.Parse(UserId);

        foreach (var item in transferOrders.Items)
        {
            item.ReceiverNameSurname = await MaskNameIfNeededAsync(item.ReceiverWalletNumber, item.ReceiverNameSurname, userGuid);
            item.SenderNameSurname = await MaskNameIfNeededAsync(item.SenderWalletNumber, item.SenderNameSurname, userGuid);
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

        var userGuid = Guid.Parse(UserId);
        transferOrder.ReceiverNameSurname = await MaskNameIfNeededAsync(transferOrder.ReceiverWalletNumber, transferOrder.ReceiverNameSurname, userGuid);
        transferOrder.SenderNameSurname = await MaskNameIfNeededAsync(transferOrder.SenderWalletNumber, transferOrder.SenderNameSurname, userGuid);

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

    private async Task<string> MaskNameIfNeededAsync(string walletNumber, string name, Guid loggedUserId)
    {
        if (string.IsNullOrEmpty(walletNumber) || string.IsNullOrEmpty(name))
        {
            return name;
        }

        var accountResponse = await GetAsync($"v1/accounts/detail?UserId={Guid.Empty}&WalletNumber={walletNumber}");
        var account = await accountResponse.Content.ReadFromJsonAsync<AccountDto>();
        var accountUsersResponse = await GetAsync($"v1/accounts/{account.Id}/users/");
        var accountUsers = await accountUsersResponse.Content.ReadFromJsonAsync<List<AccountUserDto>>();

        if (account?.IsNameMaskingEnabled == true && !accountUsers.Any(x => x.UserId == loggedUserId))
        {
            return await _stringMasking.MaskStringAsync(name);
        }

        return name;
    }
}