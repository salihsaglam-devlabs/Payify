using AutoMapper;
using LinkPara.Audit;
using LinkPara.Emoney.Application.Commons.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using LinkPara.HttpProviders;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Json;
using LinkPara.Emoney.Application.Commons.Enums;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Logging.Enums;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Logging;
using MassTransit;
using LinkPara.Emoney.Application.Features.OpenBankingOperations;
using LinkPara.ApiGateway.Services.Emoney.Models.Enums;
using LinkPara.Emoney.Application.Features.OpenBankingOperations.Commands.CreateAccountConsent;
using LinkPara.Emoney.Application.Features.OpenBankingOperations.Queries.GetHhsAccessToken;
using LinkPara.Emoney.Application.Features.OpenBankingOperations.Queries.GetAccountConsent;
using LinkPara.Emoney.Application.Features.OpenBankingOperations.Commands.DeleteAccountConsent;
using LinkPara.Emoney.Application.Features.OpenBankingOperations.Queries.GetActiveAccountConsentList;
using LinkPara.Emoney.Application.Features.OpenBankingOperations.Queries.GetConsentedAccountList;
using LinkPara.Emoney.Application.Features.OpenBankingOperations.Queries.GetConsentedAccountDetail;
using LinkPara.Emoney.Application.Commons.Models.OpenBankingModels;
using LinkPara.HttpProviders.Utility;
using LinkPara.Emoney.Application.Features.OpenBankingOperations.Queries.GetConsentedAccountBalanceList;
using LinkPara.Emoney.Application.Features.OpenBankingOperations.Queries.GetConsentedAccountBalanceDetail;
using LinkPara.Emoney.Application.Features.OpenBankingOperations.Queries.GetConsentedAccountActivities;
using LinkPara.ApiGateway.Services.Emoney.Models.Requests;
using LinkPara.Emoney.Application.Features.OpenBankingOperations.Commands.CreatePaymentConsent;
using LinkPara.Emoney.Application.Features.OpenBankingOperations.Queries.GetPaymentOrderConsentDetail;
using LinkPara.Emoney.Application.Features.OpenBankingOperations.Commands.CreatePaymentOrder;
using LinkPara.Emoney.Application.Features.OpenBankingOperations.Queries.PaymentOrderDetail;
using LinkPara.Emoney.Application.Commons.Models.ConsentModels.Responses;
using LinkPara.Cache;
using Newtonsoft.Json.Linq;

namespace LinkPara.Emoney.Infrastructure.Services;

public class OpenBankingOperationsService : HttpClientBase, IOpenBankingOperationsService
{

    private readonly HttpClient _client;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AccountService> _logger;
    private readonly IMapper _mapper;
    private readonly IBus _bus;
    private readonly ISecretService _secretService;
    private readonly ICacheService _cacheService;

    public OpenBankingOperationsService(
        HttpClient client,
        IHttpContextAccessor httpContextAccessor,
        ILogger<AccountService> logger,
        IMapper mapper,
        IBus bus,
        ISecretService secretService,
        ICacheService cacheService)
        : base(client, httpContextAccessor)
    {
        _client = client;
        _client.BaseAddress = new Uri(secretService.OpenBankingYosSettings.OpenBankingUrl);
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
        _mapper = mapper;
        _bus = bus;
        _secretService = secretService;
        _cacheService = cacheService;
    }

    private async Task<string> GetAirapiAccessTokenAsync()
    {
        if (await _cacheService.ContainsKeyAsync<string>("OpenBankingYosToken"))
        {
            return _cacheService.Get<string>("OpenBankingYosToken");
        }

        var client = new HttpClient()
        {
            BaseAddress = new Uri(_secretService.OpenBankingYosSettings.Authority)
        };

        var collection = new List<KeyValuePair<string, string>>(){
                new("client_id", _secretService.OpenBankingYosSettings.ClientId),
                new("client_secret", _secretService.OpenBankingYosSettings.ClientSecret),
                new("scope", _secretService.OpenBankingYosSettings.Scope),
                new("grant_type", _secretService.OpenBankingYosSettings.GrantType)
            };
        var content = new FormUrlEncodedContent(collection);

        var response = await client.PostAsync("connect/token", content);

        var accessTokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>();

        var tokenStr = accessTokenResponse != null ? $"{accessTokenResponse.Token_Type} {accessTokenResponse.Access_Token}" : string.Empty;

        var cacheVal = await _cacheService.GetOrCreateAsync<string>("OpenBankingYosToken", () => { return Task.FromResult(tokenStr); },
            (accessTokenResponse.Expires_In - 60) / 60);

        return cacheVal;
    }

    public async Task<HhsResultDto> GetHhsListAsync()
    {
        var token = await GetAirapiAccessTokenAsync();
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", token);
        _client.DefaultRequestHeaders.Add("PSU-Initiated", "E");

        var hhsListResult = await YosServiceGetAsync<HhsResultDto>("ohvps/hbh/s1.0/bankalar", string.Empty, "GetHhsList");

        return hhsListResult;
    }

    public async Task<AccountConsentDetailResultDto> CreateAccountConsentAsync(CreateAccountConsentCommand command)
    {
        var token = await GetAirapiAccessTokenAsync();
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", token);
        _client.DefaultRequestHeaders.Add("PSU-Initiated", "E");
        _client.DefaultRequestHeaders.Add("x-aspsp-code", command.HhsCode);
        _client.DefaultRequestHeaders.Add("x-app-user-id", command.AppUserId.ToString());
        _client.DefaultRequestHeaders.Add("x-request-id", Guid.NewGuid().ToString());

        var request = new CreateAccountConsentRequest()
        {
            YonTipi = command.ForwardType == YosForwardType.Mobil ? "M" : "W",
            ErisimIzniSonTrh = command.AccessExpireDate.ToLongDateString(),
            IznTur = command.PermissionTypes,
            DrmKod = command.StatusCode,
            Kmlk = command.Identity
        };

        string logReq = Newtonsoft.Json.JsonConvert.SerializeObject(command);
        var createAccountConsentResult = await YosServicePostAsync<AccountConsentDetailResultDto>("ohvps/hbh/s1.0/hesap-bilgisi-rizasi", request, logReq, "CreateAccountConsent");

        return createAccountConsentResult;
    }
    public async Task<YosServiceResultDto> GetHhsAccessTokenAsync(GetHhsAccessTokenQuery query)
    {
        var token = await GetAirapiAccessTokenAsync();
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", token);
        _client.DefaultRequestHeaders.Add("PSU-Initiated", "E");
        _client.DefaultRequestHeaders.Add("x-request-id", Guid.NewGuid().ToString());

        var request = new GetHhsAccessTokenRequest()
        {
            RizaNo = query.ConsentId,
            RizaTip = query.ConsentType == ConsentType.PaymentOrder ? "O" : "H",
            YetTip = "yetkod",
            YetKod = query.AccessCode,
        };

        string logReq = Newtonsoft.Json.JsonConvert.SerializeObject(query);
        var accessTokenResponse = await YosServicePostAsync<YosServiceResultDto>("gkd/s1.0/erisim-belirteci", request, logReq, "GetHhsAccessToken");

        return accessTokenResponse;

    }
    public async Task<AccountConsentDetailResultDto> GetAccountConsentDetailAsync(GetAccountConsentQuery query)
    {
        var token = await GetAirapiAccessTokenAsync();
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", token);
        _client.DefaultRequestHeaders.Add("PSU-Initiated", "E");
        _client.DefaultRequestHeaders.Add("x-request-id", Guid.NewGuid().ToString());

        string logReq = Newtonsoft.Json.JsonConvert.SerializeObject(query);
        var consentDetailResult = await YosServiceGetAsync<AccountConsentDetailResultDto>($"ohvps/hbh/s1.0/hesap-bilgisi-rizasi/{query.ConsentId}", logReq, "GetAccountConsentDetail");

        return consentDetailResult;
    }
    public async Task<YosServiceResultDto> DeleteAccountConsentAsync(DeleteAccountConsentCommand command)
    {
        try
        {
            var token = await GetAirapiAccessTokenAsync();
            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Add("Authorization", token);
            _client.DefaultRequestHeaders.Add("PSU-Initiated", "E");
            _client.DefaultRequestHeaders.Add("x-request-id", Guid.NewGuid().ToString());

            var correlationId = Guid.NewGuid();

            string logReq = Newtonsoft.Json.JsonConvert.SerializeObject(command);

            await SendIntegrationRequest(logReq, "DeleteAccountConsent", correlationId, IntegrationLogDataType.Json);

            var response = await _client.DeleteAsync($"ohvps/hbh/s1.0/hesap-bilgisi-rizasi/{command.ConsentId}");

            await SendIntegrationResponse("DeleteAccountConsent", response, correlationId);

            var responseString = await response.Content.ReadAsStringAsync();
            var deleteConsentResult = JsonSerializer.Deserialize<YosServiceResultDto>(responseString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return deleteConsentResult;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Consent Operation Get Error: {ex.Message}");
            throw;
        }
    }
    public async Task<ActiveAccountConsentResultDto> GetActiveAccountConsentListAsync(GetActiveAccountConsentListQuery query)
    {
        var token = await GetAirapiAccessTokenAsync();
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", token);
        _client.DefaultRequestHeaders.Add("PSU-Initiated", "E");
        _client.DefaultRequestHeaders.Add("x-request-id", Guid.NewGuid().ToString());
        _client.DefaultRequestHeaders.Add("x-cache-update", Boolean.FalseString);
        _client.DefaultRequestHeaders.Add("x-app-user-id", query.AppUserId.ToString());

        string logReq = Newtonsoft.Json.JsonConvert.SerializeObject(query);
        var activeAccountConsentListResult = await YosServiceGetAsync<ActiveAccountConsentResultDto>($"ohvps/hbh/s1.0/myaccountconsentlist", logReq, "GetActiveAccountConsentList");

        return activeAccountConsentListResult;
    }

    public async Task<ConsentedAccountsResultDto> GetConsentedAccountListAsync(GetConsentedAccountListQuery query)
    {
        var token = await GetAirapiAccessTokenAsync();
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", token);
        _client.DefaultRequestHeaders.Add("PSU-Initiated", "E");
        _client.DefaultRequestHeaders.Add("x-request-id", Guid.NewGuid().ToString());
        _client.DefaultRequestHeaders.Add("x-cache-update", Boolean.FalseString);
        _client.DefaultRequestHeaders.Add("x-app-user-id", query.AppUserId.ToString());
        _client.DefaultRequestHeaders.Add("x-aspsp-code", query.HhsCode);

        string logReq = Newtonsoft.Json.JsonConvert.SerializeObject(query);
        var consentedAccountsResult = await YosServiceGetAsync<ConsentedAccountsResultDto>($"ohvps/hbh/s1.0/hesaplar", logReq, "GetConsentedAccountList");

        return consentedAccountsResult;
    }

    public async Task<ConsentedAccountDetailResultDto> GetConsentedAccountDetailAsync(GetConsentedAccountDetailQuery query)
    {
        var token = await GetAirapiAccessTokenAsync();
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", token);
        _client.DefaultRequestHeaders.Add("PSU-Initiated", "E");
        _client.DefaultRequestHeaders.Add("x-request-id", Guid.NewGuid().ToString());
        _client.DefaultRequestHeaders.Add("x-cache-update", Boolean.FalseString);
        _client.DefaultRequestHeaders.Add("x-app-user-id", query.AppUserId.ToString());
        _client.DefaultRequestHeaders.Add("x-aspsp-code", query.HhsCode);

        string logReq = Newtonsoft.Json.JsonConvert.SerializeObject(query);
        var consentedAccountDetailResult = await YosServiceGetAsync<ConsentedAccountDetailResultDto>($"ohvps/hbh/s1.0/hesaplar/{query.AccountReference}", logReq, "GetConsentedAccountDetail");

        return consentedAccountDetailResult;
    }

    public async Task<ConsentedAccountBalancesResultDto> GetConsentedAccountBalanceListAsync(GetConsentedAccountBalanceListQuery query)
    {
        var token = await GetAirapiAccessTokenAsync();
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", token);
        _client.DefaultRequestHeaders.Add("PSU-Initiated", "E");
        _client.DefaultRequestHeaders.Add("x-request-id", Guid.NewGuid().ToString());
        _client.DefaultRequestHeaders.Add("x-cache-update", Boolean.FalseString);
        _client.DefaultRequestHeaders.Add("x-app-user-id", query.AppUserId.ToString());
        _client.DefaultRequestHeaders.Add("x-aspsp-code", query.HhsCode);

        string logReq = Newtonsoft.Json.JsonConvert.SerializeObject(query);
        var consentedAccountBalancesResult = await YosServiceGetAsync<ConsentedAccountBalancesResultDto>($"ohvps/hbh/s1.0/bakiye", logReq, "GetConsentedAccountBalanceList");

        foreach (var result in consentedAccountBalancesResult.Result)
        {
            result.Bky.BkyZmnDeger = DateTime.Parse(result.Bky.BkyZmn);
        }

        return consentedAccountBalancesResult;
    }

    public async Task<ConsentedAccountBalanceDetailResultDto> GetConsentedAccountBalanceDetailAsync(GetConsentedAccountBalanceDetailQuery query)
    {
        var token = await GetAirapiAccessTokenAsync();
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", token);
        _client.DefaultRequestHeaders.Add("PSU-Initiated", "E");
        _client.DefaultRequestHeaders.Add("x-request-id", Guid.NewGuid().ToString());
        _client.DefaultRequestHeaders.Add("x-cache-update", Boolean.FalseString);
        _client.DefaultRequestHeaders.Add("x-app-user-id", query.AppUserId.ToString());
        _client.DefaultRequestHeaders.Add("x-aspsp-code", query.HhsCode);

        string logReq = Newtonsoft.Json.JsonConvert.SerializeObject(query);
        var consentedAccountBalanceDetailResult = await YosServiceGetAsync<ConsentedAccountBalanceDetailResultDto>($"ohvps/hbh/s1.0/hesaplar/{query.AccountReference}/bakiye", logReq, "GetConsentedAccountBalanceDetail");

        return consentedAccountBalanceDetailResult;
    }

    public async Task<ConsentedAccountActivitiesResultDto> GetConsentedAccountActivitiesAsync(GetConsentedAccountActivitiesQuery query)
    {
        var token = await GetAirapiAccessTokenAsync();
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", token);
        _client.DefaultRequestHeaders.Add("PSU-Initiated", "E");
        _client.DefaultRequestHeaders.Add("x-request-id", Guid.NewGuid().ToString());
        string logReq = Newtonsoft.Json.JsonConvert.SerializeObject(query);

        var request = _mapper.Map<GetConsentedAccountActivitiesRequest>(query);
        var queryString = GetQueryString.CreateUrlWithParams($"ohvps/hbh/s1.0/hesaplar/{query.AccountReference}/islemler", request);
        var consentedAccountActivitiesResult = await YosServiceGetAsync<ConsentedAccountActivitiesResultDto>(queryString, logReq, "GetConsentedAccountActivities");

        return consentedAccountActivitiesResult;
    }

    public async Task<PaymentOrderConsentDetailDto> CreatePaymentConsentAsync(CreatePaymentConsentCommand command)
    {
        var token = await GetAirapiAccessTokenAsync();
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", token);
        _client.DefaultRequestHeaders.Add("PSU-Initiated", "E");
        _client.DefaultRequestHeaders.Add("x-request-id", Guid.NewGuid().ToString());
        _client.DefaultRequestHeaders.Add("x-app-user-id", command.AppUserId.ToString());
        _client.DefaultRequestHeaders.Add("x-aspsp-code", command.HhsCode);

        var request = _mapper.Map<CreatePaymentConsentRequest>(command);
        request.YonTipi = command.YonTipi == YosForwardType.Mobil ? "M" : "W";

        string logReq = Newtonsoft.Json.JsonConvert.SerializeObject(command);
        var paymentOrderConsentDetailResult = await YosServicePostAsync<PaymentOrderConsentDetailDto>("ohvps/obh/s1.0/odeme-emri-rizasi", request, logReq, "CreatePaymentConsent");

        return paymentOrderConsentDetailResult;
    }

    public async Task<PaymentOrderConsentDetailDto> GetPaymentOrderConsentDetailAsync(GetPaymentOrderConsentDetailQuery query)
    {
        var token = await GetAirapiAccessTokenAsync();
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", token);
        _client.DefaultRequestHeaders.Add("PSU-Initiated", "E");
        _client.DefaultRequestHeaders.Add("x-request-id", Guid.NewGuid().ToString());
        _client.DefaultRequestHeaders.Add("x-app-user-id", query.AppUserId.ToString());
        _client.DefaultRequestHeaders.Add("x-aspsp-code", query.HhsCode);

        string logReq = Newtonsoft.Json.JsonConvert.SerializeObject(query);
        var paymentOrderConsentDetailResult = await YosServiceGetAsync<PaymentOrderConsentDetailDto>($"ohvps/obh/s1.0/odeme-emri-rizasi/{query.ConsentId}", logReq, "GetPaymentOrderConsentDetail");

        return paymentOrderConsentDetailResult;
    }

    public async Task<PaymentOrderDetailResultDto> CreatePaymentOrderAsync(CreatePaymentOrderYosCommand command)
    {
        var token = await GetAirapiAccessTokenAsync();
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", token);
        _client.DefaultRequestHeaders.Add("PSU-Initiated", "E");
        _client.DefaultRequestHeaders.Add("x-request-id", Guid.NewGuid().ToString());
        _client.DefaultRequestHeaders.Add("x-app-user-id", command.AppUserId.ToString());
        _client.DefaultRequestHeaders.Add("x-aspsp-code", command.HhsCode);
        _client.DefaultRequestHeaders.Add("x-riza-no", command.ConsentId);

        string logReq = Newtonsoft.Json.JsonConvert.SerializeObject(command);

        var paymentOrderResult = await YosServicePostAsync<PaymentOrderDetailResultDto>("ohvps/obh/s1.0/odeme-emri", new { }, logReq, "CreatePaymentOrder");

        return paymentOrderResult;
    }

    public async Task<PaymentOrderDetailResultDto> PaymentOrderDetailQueryAsync(PaymentOrderDetailQuery query)
    {
        var token = await GetAirapiAccessTokenAsync();
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", token);
        _client.DefaultRequestHeaders.Add("PSU-Initiated", "E");
        _client.DefaultRequestHeaders.Add("x-request-id", Guid.NewGuid().ToString());
        _client.DefaultRequestHeaders.Add("x-app-user-id", query.AppUserId.ToString());
        _client.DefaultRequestHeaders.Add("x-aspsp-code", query.HhsCode);
        _client.DefaultRequestHeaders.Add("x-riza-no", query.ConsentId);
        _client.DefaultRequestHeaders.Add("isPaymentOrConsentId", "1");

        string logReq = Newtonsoft.Json.JsonConvert.SerializeObject(query);

        var paymentOrderResult = await YosServicePostAsync<PaymentOrderDetailResultDto>($"ohvps/obh/s1.0/odeme-emri/{query.ConsentId}", new { }, logReq, "PaymentOrderDetailQuery");

        return paymentOrderResult;
    }

    private async Task<T> YosServiceGetAsync<T>(string queryString, string jsonData, string processName)
    {
        try
        {
            var correlationId = Guid.NewGuid();

            await SendIntegrationRequest(jsonData, processName, correlationId, IntegrationLogDataType.Json);

            var response = await _client.GetAsync(queryString);

            await SendIntegrationResponse(processName, response, correlationId);

            var deserializedResponse = await response.Content.ReadFromJsonAsync<T>();

            return deserializedResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,$"YosService Operation Get Error: {ex.Message}");
            throw;
        }

    }
    private async Task<T> YosServicePostAsync<T>(string url, object data, string jsonData, string processName)
    {
        try
        {
            var correlationId = Guid.NewGuid();

            await SendIntegrationRequest(jsonData, processName, correlationId, IntegrationLogDataType.Json);

            var response = await _client.PostAsJsonAsync(url, data);

            await SendIntegrationResponse(processName, response, correlationId);

            var deserializedResponse = await response.Content.ReadFromJsonAsync<T>();

            return deserializedResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Yos Service Operation Post Error: {ex.Message}");
            throw;
        }

    }

    private async Task SendIntegrationRequest(string data, string logName, Guid correlationId, IntegrationLogDataType integrationLogDataType)
    {
        try
        {
            var log = new IntegrationLog()
            {
                CorrelationId = correlationId.ToString(),
                Name = logName,
                Type = nameof(IntegrationLogType.OpenBanking),
                Date = DateTime.Now,
                Request = data,
                DataType = integrationLogDataType
            };

            var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:Log.IntegrationLog"));
            await endpoint.Send(log, cancellationToken.Token);

        }
        catch (Exception exception)
        {
            _logger.LogError(exception,$"Send Yos Service Request Data To Integration Log Error: {exception}");
        }

    }

    private async Task SendIntegrationResponse(string logName, HttpResponseMessage httpResponse, Guid correlationId)
    {
        try
        {
            var log = new IntegrationLog()
            {
                CorrelationId = correlationId.ToString(),
                Name = logName,
                Type = nameof(IntegrationLogType.OpenBanking),
                Date = DateTime.Now,
                Response = await httpResponse.Content.ReadAsStringAsync(),
                HttpCode = ((int)httpResponse.StatusCode).ToString(),
                ErrorCode = httpResponse.StatusCode.ToString(),
                ErrorMessage = httpResponse.StatusCode.ToString(),
                DataType = IntegrationLogDataType.Json
            };

            var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:Log.IntegrationLog"));
            await endpoint.Send(log, cancellationToken.Token);
        }
        catch (Exception exception)
        {
            _logger.LogError($"Send Yos Service Response Data To Integration Log Error: {exception}");
        }

    }

}
