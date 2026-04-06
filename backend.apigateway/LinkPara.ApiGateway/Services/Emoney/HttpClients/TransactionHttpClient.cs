using LinkPara.ApiGateway.Commons.Extensions;
using LinkPara.ApiGateway.Commons.Helpers;
using LinkPara.ApiGateway.Services.Emoney.Models.Enums;
using LinkPara.ApiGateway.Services.Emoney.Models.Requests;
using LinkPara.ApiGateway.Services.Emoney.Models.Responses;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Pagination;
using System.Security.Claims;
using System.Text.Json;

namespace LinkPara.ApiGateway.Services.Emoney.HttpClients;

public class TransactionHttpClient : HttpClientBase, ITransactionHttpClient
{
    private readonly IServiceRequestConverter _serviceRequestConverter;
    private readonly IWalletHttpClient _walletHttpClient;
    private readonly IStringMasking _stringMasking;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private Guid UserId;

    public TransactionHttpClient(HttpClient client,
        IHttpContextAccessor httpContextAccessor,
        IServiceRequestConverter serviceRequestConverter,
        IWalletHttpClient walletHttpClient,
        IStringMasking stringMasking)
        : base(client, httpContextAccessor)
    {
        _serviceRequestConverter = serviceRequestConverter;
        _walletHttpClient = walletHttpClient;
        _stringMasking = stringMasking;
        _httpContextAccessor = httpContextAccessor;
        UserId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier) is not null
            ? Guid.TryParse(_httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier), out var userId) ? userId : Guid.Empty
            : Guid.Empty;
    }

    public async Task<TransactionDto> GetTransactionDetailsAsync(Guid id)
    {
        var response = await GetAsync($"v1/transactions/" + id);
        var transaction = await response.Content.ReadFromJsonAsync<TransactionDto>();

        transaction = await CheckMaskingNeed(transaction);

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


        for (int i = 0; i < transactions.Items.Count; i++)
        {
            transactions.Items[i] = await CheckMaskingNeed(transactions.Items[i]);
        }

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

    public async Task<PaginatedList<TransactionDto>> GetCustodyWalletTransactionsAsync(GetCustodyWalletTransactionsRequest request)
    {
        var queryString = request.GetQueryString();

        var response = await GetAsync($"v1/transactions/getCustodyWalletTransactions" + queryString);
        var transactions = await response.Content.ReadFromJsonAsync<PaginatedList<TransactionDto>>();


        for (int i = 0; i < transactions.Items.Count; i++)
        {
            transactions.Items[i] = await CheckMaskingNeed(transactions.Items[i]);
        }

        return transactions ?? throw new InvalidOperationException();
    }

    public async Task<FastTransactionAmountsDto> GetUserFastTransactionAmountsAsync(Guid walletId)
    {
        var response = await GetAsync($"v1/transactions/user-fast-transaction-amounts/{walletId}");
        var fastTransactionAmounts = await response.Content.ReadFromJsonAsync<FastTransactionAmountsDto>();
        return fastTransactionAmounts ?? throw new InvalidOperationException();
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

    private async Task<TransactionDto> CheckMaskingNeed(TransactionDto transaction)
    {
        if (transaction.PaymentMethod == PaymentMethod.Transfer)
        {
            if (transaction.TransactionType == TransactionType.Withdraw)
            {
                var newSenderName = await MaskNameIfNeededAsync(transaction.WalletNumber, transaction.SenderName, UserId);
                if (transaction.Tag.Contains(transaction.SenderName))
                {
                    transaction.Tag = transaction.Tag.Replace(transaction.SenderName, newSenderName);
                }
                transaction.SenderName = newSenderName;
                transaction.WalletName = newSenderName;
                var newReceiverName = await MaskNameIfNeededAsync(transaction.CounterWalletNumber, transaction.ReceiverName, UserId);
                if (transaction.Tag.Contains(transaction.ReceiverName))
                {
                    transaction.Tag = transaction.Tag.Replace(transaction.ReceiverName, newReceiverName);
                }
                transaction.ReceiverName = newReceiverName;
                transaction.CounterWalletName = newReceiverName;
            }
            else if (transaction.TransactionType == TransactionType.Deposit)
            {
                var newSenderName = await MaskNameIfNeededAsync(transaction.CounterWalletNumber, transaction.SenderName, UserId);
                if (transaction.Tag.Contains(transaction.SenderName))
                {
                    transaction.Tag = transaction.Tag.Replace(transaction.SenderName, newSenderName);
                }
                transaction.SenderName = newSenderName;
                transaction.CounterWalletName = newSenderName;

                var newReceiverName = await MaskNameIfNeededAsync(transaction.WalletNumber, transaction.ReceiverName, UserId);
                if (transaction.Tag.Contains(transaction.ReceiverName))
                {
                    transaction.Tag = transaction.Tag.Replace(transaction.ReceiverName, newReceiverName);
                }
                transaction.ReceiverName = newReceiverName;
                transaction.WalletName = newReceiverName;

            }
            else if (transaction.TransactionType == TransactionType.Commission || transaction.TransactionType == TransactionType.Tax)
            {
                var parentTransaction = await GetTransactionDetailsAsync(transaction.RelatedTransactionId ?? Guid.Empty);
                var newSenderName = await MaskNameIfNeededAsync(parentTransaction.WalletNumber, transaction.SenderName, UserId);
                if (!string.IsNullOrEmpty(transaction.SenderName) && transaction.Tag.Contains(transaction.SenderName))
                {
                    transaction.Tag = transaction.Tag.Replace(transaction.SenderName, newSenderName);
                }
                transaction.SenderName = newSenderName;
                transaction.WalletName = newSenderName;
                var newReceiverName = await MaskNameIfNeededAsync(parentTransaction.CounterWalletNumber, transaction.ReceiverName, UserId);
                if (!string.IsNullOrEmpty(transaction.ReceiverName) && transaction.Tag.Contains(transaction.ReceiverName))
                {
                    transaction.Tag = transaction.Tag.Replace(transaction.ReceiverName, newReceiverName);
                }
                transaction.ReceiverName = newReceiverName;
                transaction.CounterWalletName = newReceiverName;
            }
        }
        return transaction;
    }
}