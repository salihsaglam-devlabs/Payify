using LinkPara.ApiGateway.Commons.Extensions;
using LinkPara.ApiGateway.Commons.Helpers;
using LinkPara.ApiGateway.Services.Emoney.Models.Requests;
using LinkPara.ApiGateway.Services.Emoney.Models.Responses;
using System.Globalization;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Web;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace LinkPara.ApiGateway.Services.Emoney.HttpClients;

public class WalletHttpClient : HttpClientBase, IWalletHttpClient
{
    private readonly IServiceRequestConverter _serviceRequestConverter;
    private readonly IStringMasking _stringMasking;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private Guid UserId;


    public WalletHttpClient(HttpClient client, IServiceRequestConverter serviceRequestConverter,
         IHttpContextAccessor httpContextAccessor, IStringMasking stringMasking)
        : base(client, httpContextAccessor)
    {
        _stringMasking = stringMasking;
        _serviceRequestConverter = serviceRequestConverter;
        _httpContextAccessor = httpContextAccessor;
        UserId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier) is not null
      ? Guid.TryParse(_httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier), out var userId) ? userId : Guid.Empty
      : Guid.Empty;
    }

    public async Task<WalletDto> GetWalletDetailsAsync(GetWalletDetailsRequest request)
    {
        var clientRequest = _serviceRequestConverter.Convert<GetWalletDetailsRequest, GetWalletDetailsServiceRequest>(request);
        var queryString = clientRequest.GetQueryString();

        var response = await GetAsync($"v1/Wallets/details" + queryString);
        var responseString = await response.Content.ReadAsStringAsync();
        var wallet = JsonSerializer.Deserialize<WalletDto>(responseString, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return wallet ?? throw new InvalidOperationException();
    }

    public async Task<List<WalletDto>> GetUserWalletsAsync(GetUserWalletsFilterRequest request)
    {
        var clientRequest = _serviceRequestConverter.Convert<GetUserWalletsFilterRequest, GetUserWalletsFilterServiceRequest>(request);
        var queryString = clientRequest.GetQueryString();

        var response = await GetAsync($"v1/Wallets" + queryString);
        var responseString = await response.Content.ReadAsStringAsync();
        var walletList = JsonSerializer.Deserialize<List<WalletDto>>(responseString, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return walletList ?? throw new InvalidOperationException();
    }

    public async Task SaveWalletAsync(SaveWalletRequest request, string userId)
    {
        var clientRequest = _serviceRequestConverter.Convert<SaveWalletRequest, SaveWalletServiceRequest>(request);

        if (!string.IsNullOrEmpty(userId))
        {
            clientRequest.UserId = Guid.Parse(userId);
        }

        var response = await PostAsJsonAsync($"v1/Wallets", clientRequest);

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException();
    }

    public async Task UpdateWalletAsync(UpdateWalletRequest request)
    {
        var clientRequest = _serviceRequestConverter.Convert<UpdateWalletRequest, UpdateWalletServiceRequest>(request);

        var requestContent = new StringContent(JsonSerializer.Serialize(clientRequest), Encoding.UTF8, "application/json");
        var response = await PatchAsync($"v1/Wallets", requestContent);

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException();
    }

    public async Task<MoneyTransferResponse> TransferAsync(TransferRequest request)
    {
        var clientRequest = _serviceRequestConverter.Convert<TransferRequest, TransferServiceRequest>(request);

        var response = await PostAsJsonAsync($"v1/Wallets/transfer", clientRequest);

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException();

        var responseString = await response.Content.ReadAsStringAsync();

        var moneyTransferResponse = JsonSerializer.Deserialize<MoneyTransferResponse>(responseString,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        return moneyTransferResponse ?? throw new InvalidOperationException();
    }

    public async Task<MoneyTransferResponse> WithdrawAsync(WithdrawRequest request)
    {
        var clientRequest = _serviceRequestConverter.Convert<WithdrawRequest, WithdrawServiceRequest>(request);

        var response = await PostAsJsonAsync($"v1/Wallets/withdraw", clientRequest);

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException();

        var responseString = await response.Content.ReadAsStringAsync();

        var moneyTransferResponse = JsonSerializer.Deserialize<MoneyTransferResponse>(responseString,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        return moneyTransferResponse ?? throw new InvalidOperationException();
    }

    public async Task<WalletSummaryDto> GetWalletSummaryAsync(GetWalletSummaryDetailsRequest request)
    {
        var queryString = request.GetQueryString();

        var response = await GetAsync($"v1/Wallets/summary" + queryString);
        var responseString = await response.Content.ReadAsStringAsync();
        var counterWallet = JsonSerializer.Deserialize<WalletSummaryDto>(responseString, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        counterWallet.UserName = await _stringMasking.MaskStringAsync(counterWallet.UserName);

        return counterWallet ?? throw new InvalidOperationException();
    }

    public async Task<WithdrawPreviewResponse> WithdrawPreviewAsync(WithdrawPreviewRequest request)
    {
        var clientRequest = _serviceRequestConverter.Convert<WithdrawPreviewRequest, WithdrawPreviewServiceRequest>(request);

        var receiverName = string.IsNullOrEmpty(request.ReceiverName) ? "" : HttpUtility.UrlEncode(request.ReceiverName);
        var description = string.IsNullOrEmpty(request.Description) ? "" : HttpUtility.UrlEncode(request.Description);

        var qs = $"UserId={clientRequest.UserId}" +
            $"&Amount={request.Amount.ToString(CultureInfo.InvariantCulture)}" +
            $"&ReceiverIBAN={request.ReceiverIBAN}" +
            $"&ReceiverName={receiverName}" +
            $"&Description={description}" +
            $"&PaymentType={request.PaymentType}" +
            $"&WalletNumber={request.WalletNumber}";

        var response = await GetAsync($"v1/Wallets/withdraw-preview?{qs}");
        var responseString = await response.Content.ReadAsStringAsync();

        var withdrawPreviewResponse = JsonSerializer.Deserialize<WithdrawPreviewResponse>(responseString, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });


        withdrawPreviewResponse.ReceiverName = await MaskNameIfNeededAsync(withdrawPreviewResponse.WalletNumber, withdrawPreviewResponse.ReceiverName, UserId);

        return withdrawPreviewResponse ?? throw new InvalidOperationException();
    }

    public async Task<TransferPreviewResponse> TransferPreviewAsync(TransferPreviewRequest request)
    {
        var clientRequest = _serviceRequestConverter.Convert<TransferPreviewRequest, TransferPreviewServiceRequest>(request);

        var description = string.IsNullOrEmpty(request.Description) ? "" : HttpUtility.UrlEncode(request.Description);

        var qs = $"UserId={clientRequest.UserId}" +
            $"&Amount={request.Amount.ToString(CultureInfo.InvariantCulture)}" +
            $"&Description={description}" +
            $"&PaymentType={request.PaymentType}" +
            $"&SenderWalletNumber={request.SenderWalletNumber}" +
            $"&ReceiverWalletNumber={request.ReceiverWalletNumber}";

        var response = await GetAsync($"v1/Wallets/transfer-preview?{qs}");
        var responseString = await response.Content.ReadAsStringAsync();
        var transferPreviewResponse = JsonSerializer.Deserialize<TransferPreviewResponse>(responseString, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });


        transferPreviewResponse.ReceiverName = await MaskNameIfNeededAsync(transferPreviewResponse.ReceiverWalletNumber, transferPreviewResponse.ReceiverName, UserId);

        return transferPreviewResponse ?? throw new InvalidOperationException();
    }


    public async Task<List<WalletDto>> GetAccountWalletsAsync(AccountWalletsRequest request)
    {
        var queryString = request.GetQueryString();
        var response = await GetAsync($"v1/Wallets/getAccountWallets" + queryString);

        return await response.Content.ReadFromJsonAsync<List<WalletDto>>();
    }

    public async Task<PayWithWalletResponse> TransferForLoggedInUserAsync(TransferForLoggedInUserRequest request)
    {
        var response = await PostAsJsonAsync($"v1/PayWithWallets/transfer-for-logged-in-user", request);
        var responseString = await response.Content.ReadAsStringAsync();
        var payWithWalletResponse = JsonSerializer.Deserialize<PayWithWalletResponse>(responseString,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return payWithWalletResponse;
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

    public async Task<List<MoneyTransferPaymentType>> GetMoneyTransferPaymentTypeAsync()
    {
        var response = await GetAsync($"v1/Wallets/MoneyTransferPaymentType");
        var moneyTransferPaymentTypes = await response.Content.ReadFromJsonAsync<List<MoneyTransferPaymentType>>();
        return moneyTransferPaymentTypes ?? throw new InvalidOperationException();
    }
}