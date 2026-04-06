using LinkPara.ApiGateway.Boa.Commons.Extensions;
using LinkPara.ApiGateway.Boa.Commons.Helpers;
using LinkPara.ApiGateway.Boa.Services.Emoney.Models.Requests;
using LinkPara.ApiGateway.Boa.Services.Emoney.Models.Responses;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Web;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace LinkPara.ApiGateway.Boa.Services.Emoney.HttpClients;

public class WalletHttpClient : HttpClientBase, IWalletHttpClient
{
    private readonly IServiceRequestConverter _serviceRequestConverter;
    private readonly IStringMasking _stringMasking;

    public WalletHttpClient(HttpClient client, IServiceRequestConverter serviceRequestConverter,
         IHttpContextAccessor httpContextAccessor, IStringMasking stringMasking) : base(client, httpContextAccessor)
    {
        _stringMasking = stringMasking;
        _serviceRequestConverter = serviceRequestConverter;
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
        transferPreviewResponse.ReceiverName = await _stringMasking.MaskStringAsync(transferPreviewResponse.ReceiverName);

        return transferPreviewResponse ?? throw new InvalidOperationException();
    }

    public async Task<List<WalletDto>> GetAccountWalletsAsync(AccountWalletsRequest request)
    {
        var queryString = request.GetQueryString();
        var response = await GetAsync($"v1/Wallets/getAccountWallets" + queryString);

        return await response.Content.ReadFromJsonAsync<List<WalletDto>>();
    }
    public async Task<UpdateBalanceResponse> UpdateBalanceAsync(UpdateBalanceRequest request)
    {
        var response = await PutAsJsonAsync($"v1/Wallets/update-balance", request);
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException();
        return await response.Content.ReadFromJsonAsync<UpdateBalanceResponse>();
    }

    public async Task<List<MoneyTransferPaymentType>> GetMoneyTransferPaymentTypeAsync()
    {
        var response = await GetAsync($"v1/Wallets/MoneyTransferPaymentType");
        var moneyTransferPaymentTypes = await response.Content.ReadFromJsonAsync<List<MoneyTransferPaymentType>>();
        return moneyTransferPaymentTypes ?? throw new InvalidOperationException();
    }
}