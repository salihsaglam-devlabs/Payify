using LinkPara.ApiGateway.Commons.Extensions;
using LinkPara.ApiGateway.Services.Emoney.Models.Requests;
using LinkPara.ApiGateway.Services.Emoney.Models.Responses;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace LinkPara.ApiGateway.Services.Emoney.HttpClients;
public class OpenBankingOperationHttpClient : HttpClientBase, IOpenBankingOperationHttpClient
{
    public OpenBankingOperationHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor)
    : base(client, httpContextAccessor)
    {
    }

    public async Task<HhsResultDto> GetHhsListAsync()
    {
        var response = await GetAsync($"v1/OpenBankingOperations/hhs-list");
        var responseString = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<HhsResultDto>(responseString, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return result;

    }
    public async Task<AccountConsentDetailResultDto> CreateAccountConsentAsync(CreateAccountConsentRequest request)
    {
        var result = await PostAsJsonAsync($"v1/OpenBankingOperations/create-account-consent", request);
        var response = await result.Content.ReadFromJsonAsync<AccountConsentDetailResultDto>();
        return response;

    }

    public async Task<YosServiceResultDto> GetHhsAccessTokenAsync(GetHhsAccessTokenRequest request)
    {
        var queryString = request.GetQueryString();
        var response = await GetAsync($"v1/OpenBankingOperations/hhs-token{queryString}");
        var responseString = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<YosServiceResultDto>(responseString, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return result;
    }

    public async Task<AccountConsentDetailResultDto> GetAccountConsentDetailAsync(string consentId)
    {
        var response = await GetAsync($"v1/OpenBankingOperations/account-consent-detail?consentId={consentId}");
        var responseString = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<AccountConsentDetailResultDto>(responseString, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return result;
    }

    public async Task<YosServiceResultDto> DeleteAccountConsentAsync(string consentId)
    {
        var result = await PostAsJsonAsync($"v1/OpenBankingOperations/delete-account-consent?consentId={consentId}", new {});
        var response = await result.Content.ReadFromJsonAsync<YosServiceResultDto>();
        return response;
    }

    public async Task<ActiveAccountConsentResultDto> GetActiveAccountConsentListAsync(GetActiveAccountConsentListRequest request)
    {
        var queryString = request.GetQueryString();
        var response = await GetAsync($"v1/OpenBankingOperations/active-account-consent{queryString}");
        var responseString = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ActiveAccountConsentResultDto>(responseString, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return result;
    }

    public async Task<ConsentedAccountsResultDto> GetConsentedAccountListAsync(GetConsentedAccountListRequest request)
    {
        var queryString = request.GetQueryString();
        var response = await GetAsync($"v1/OpenBankingOperations/consented-accounts{queryString}");
        var responseString = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ConsentedAccountsResultDto>(responseString, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return result;
    }

    public async Task<ConsentedAccountDetailResultDto> GetConsentedAccountDetailAsync(GetConsentedAccountDetailRequest request)
    {
        var queryString = request.GetQueryString();
        var response = await GetAsync($"v1/OpenBankingOperations/consented-account-detail{queryString}");
        var responseString = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ConsentedAccountDetailResultDto>(responseString, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return result;
    }
    public async Task<ConsentedAccountBalancesResultDto> GetConsentedAccountBalanceListAsync(GetConsentedAccountBalanceListRequest request)
    {
        var queryString = request.GetQueryString();
        var response = await GetAsync($"v1/OpenBankingOperations/account-balances{queryString}");
        var responseString = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ConsentedAccountBalancesResultDto>(responseString, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return result;
    }

    public async Task<ConsentedAccountBalanceDetailResultDto> GetConsentedAccountBalanceDetailAsync(GetConsentedAccountBalanceDetailRequest request)
    {
        var queryString = request.GetQueryString();
        var response = await GetAsync($"v1/OpenBankingOperations/account-balance-detail{queryString}");
        var responseString = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ConsentedAccountBalanceDetailResultDto>(responseString, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return result;
    }

    public async Task<ConsentedAccountActivitiesResultDto> GetConsentedAccountActivitiesAsync(GetConsentedAccountActivitiesRequest request)
    {
        var queryString = request.GetQueryString();
        var response = await GetAsync($"v1/OpenBankingOperations/account-activities{queryString}");
        var responseString = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ConsentedAccountActivitiesResultDto>(responseString, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return result;
    }

    public async Task<PaymentOrderConsentDetailDto> CreatePaymentConsentAsync(CreatePaymentConsentRequest request)
    {
        var result = await PostAsJsonAsync($"v1/OpenBankingOperations/create-payment-consent", request);
        var response = await result.Content.ReadFromJsonAsync<PaymentOrderConsentDetailDto>();
        return response;

    }

    public async Task<PaymentOrderConsentDetailDto> GetPaymentOrderConsentDetailAsync(GetPaymentOrderConsentDetailRequest request)
    {
        var queryString = request.GetQueryString();
        var response = await GetAsync($"v1/OpenBankingOperations/payment-order-consent{queryString}");
        var responseString = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PaymentOrderConsentDetailDto>(responseString, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return result;
    }

    public async Task<PaymentOrderDetailResultDto> CreatePaymentOrderAsync(CreatePaymentOrderRequest request)
    {
        var result = await PostAsJsonAsync($"v1/OpenBankingOperations/create-payment-order", request);
        var response = await result.Content.ReadFromJsonAsync<PaymentOrderDetailResultDto>();
        return response;
    }

    public async Task<PaymentOrderDetailResultDto> PaymentOrderDetailQueryAsync(PaymentOrderDetailQueryRequest request)
    {
        var queryString = request.GetQueryString();
        var response = await GetAsync($"v1/OpenBankingOperations/payment-order-detail{queryString}");
        var responseString = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PaymentOrderDetailResultDto>(responseString, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return result;
    }
       
    public async Task<ActionResult<CardsResultDto>> GetCardsAsync(GetCardsRequest request)
    {
        var queryString = request.GetQueryString();
        var response = await GetAsync($"v1/OpenBankingOperations/cards{queryString}");
        var responseString = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<CardsResultDto>(responseString, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return result;
    }

    public async Task<ActionResult<CardDetailResultDto>> GetCardDetailAsync([FromQuery] GetCardDetailRequest request)
    {
        var queryString = request.GetQueryString();
        var response = await GetAsync($"v1/OpenBankingOperations/card-detail{queryString}");
        var responseString = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<CardDetailResultDto>(responseString, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return result;
    }

    public async Task<ActionResult<CardTransactionsResultDto>> GetCardTransactionAsync([FromQuery] GetCardTransactionsRequest request)
    {
        var queryString = request.GetQueryString();
        var response = await GetAsync($"v1/OpenBankingOperations/card-transactions{queryString}");
        var responseString = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<CardTransactionsResultDto>(responseString, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return result;
    }

    public async Task<ActionResult<FuturePaymentOrderConsentResultDto>> CreateFuturePaymentOrderConsentAsync([FromBody] CreateFuturePaymentOrderConsentRequest request)
    {
        var queryString = request.GetQueryString();
        var response = await GetAsync($"v1/OpenBankingOperations/create-future-payment-order-consent{queryString}");
        var responseString = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<FuturePaymentOrderConsentResultDto>(responseString, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return result;
    }

    public async Task<ActionResult<GetFuturePaymentOrderListResultDto>> GetFuturePaymentOrderListAsync([FromBody] GetFuturePaymentOrderListRequest request)
    {
        var queryString = request.GetQueryString();
        var response = await GetAsync($"v1/OpenBankingOperations/list-future-payment-order{queryString}");
        var responseString = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<GetFuturePaymentOrderListResultDto>(responseString, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return result;
    }

    public async Task<ActionResult<TriggerFuturePaymentOrderResultDto>> TriggerFuturePaymentOrderAsync([FromBody] TriggerFuturePaymentOrderRequest request)
    {
        var queryString = request.GetQueryString();
        var response = await GetAsync($"v1/OpenBankingOperations/trigger-future-payment-order{queryString}");
        var responseString = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<TriggerFuturePaymentOrderResultDto>(responseString, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return result;
    }

    public async Task<ActionResult<CancelFuturePaymentOrderResultDto>> CancelFuturePaymentOrderAsync([FromBody] CancelFuturePaymentOrderRequest request)
    {
        var queryString = request.GetQueryString();
        var response = await GetAsync($"v1/OpenBankingOperations/cancel-future-payment-order{queryString}");
        var responseString = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<CancelFuturePaymentOrderResultDto>(responseString, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return result;
    }

    public async Task<ActionResult<StandingPaymentOrderConsentResultDto>> CreateStandingPaymentOrderConsentAsync([FromBody] CreateStandingPaymentOrderConsentRequest request)
    {
        var queryString = request.GetQueryString();
        var response = await GetAsync($"v1/OpenBankingOperations/create-standing-payment-order-consent{queryString}");
        var responseString = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<StandingPaymentOrderConsentResultDto>(responseString, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return result;
    }
}