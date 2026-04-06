using LinkPara.ApiGateway.Boa.Commons.Extensions;
using LinkPara.ApiGateway.Boa.Commons.Helpers;
using LinkPara.ApiGateway.Boa.Services.Emoney.Models.Requests;
using LinkPara.ApiGateway.Boa.Services.Emoney.Models.Responses;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Pagination;
using System.Text.Json;

namespace LinkPara.ApiGateway.Boa.Services.Emoney.HttpClients;

public class TransactionHttpClient : HttpClientBase, ITransactionHttpClient
{
    private readonly IServiceRequestConverter _serviceRequestConverter;
    private readonly IWalletHttpClient _walletHttpClient;

    public TransactionHttpClient(HttpClient client,
        IHttpContextAccessor httpContextAccessor,
        IServiceRequestConverter serviceRequestConverter,
        IWalletHttpClient walletHttpClient) : base(client, httpContextAccessor)
    {
        _serviceRequestConverter = serviceRequestConverter;
        _walletHttpClient = walletHttpClient;
    }

    public async Task<TransactionDto> GetTransactionDetailsAsync(Guid id)
    {
        var response = await GetAsync($"v1/transactions/" + id);
        var transaction = await response.Content.ReadFromJsonAsync<TransactionDto>();
        return transaction ?? throw new InvalidOperationException();
    }

    public async Task<PaginatedList<TransactionDto>> GetWalletTransactionsAsync(GetWalletTransactionsRequest request)
    {
        var wallet = await _walletHttpClient.GetWalletDetailsAsync(new GetWalletDetailsRequest
        {
            WalletId = request.WalletId,
        });

        if (wallet is null)
        {
            throw new NotFoundException(nameof(WalletDto), request.WalletId);
        }

        var url = CreateUrlWithParams($"v1/wallets/{request.WalletId}/transactions", request, true);
        var response = await GetAsync(url);
        var transactions = await response.Content.ReadFromJsonAsync<PaginatedList<TransactionDto>>();
        return transactions ?? throw new InvalidOperationException();
    }
    public async Task<TransactionSummaryDto> GetTransactionSummaryAsync(TransactionSummaryRequest request)
    {
        var queryString = request.GetQueryString();

        var clientRequest = _serviceRequestConverter.Convert<TransactionSummaryRequest, TrancationSummaryServiceRequest>(request);

        var response = await GetAsync($"v1/transactions/{clientRequest.UserId}/summary/" + queryString);
        var responseString = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<TransactionSummaryDto>(responseString, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return result ?? throw new InvalidOperationException();
    }

    public async Task<ReceiptDto> GetReceiptAsync(GetReceiptRequest request)
    {
        var queryString = request.GetQueryString();

        var clientRequest = _serviceRequestConverter.Convert<GetReceiptRequest, GetReceiptServiceRequest>(request);

        var response = await GetAsync($"v1/transactions/{clientRequest.UserId}/receipt/" + queryString);
        var responseString = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ReceiptDto>(responseString, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return result ?? throw new InvalidOperationException();
    }
}