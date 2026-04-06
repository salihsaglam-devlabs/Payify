using AutoMapper;
using LinkPara.Emoney.Application.Commons.Interfaces;
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
using LinkPara.Emoney.Application.Features.OpenBankingOperations.Queries.GetConsentedAccountBalanceList;
using LinkPara.Emoney.Application.Features.OpenBankingOperations.Queries.GetConsentedAccountBalanceDetail;
using LinkPara.Emoney.Application.Features.OpenBankingOperations.Queries.GetConsentedAccountActivities;
using LinkPara.Emoney.Application.Features.OpenBankingOperations.Commands.CreatePaymentConsent;
using LinkPara.Emoney.Application.Features.OpenBankingOperations.Queries.GetPaymentOrderConsentDetail;
using LinkPara.Emoney.Application.Features.OpenBankingOperations.Commands.CreatePaymentOrder;
using LinkPara.Emoney.Application.Features.OpenBankingOperations.Queries.PaymentOrderDetail;
using LinkPara.Emoney.Application.Commons.Models.ConsentModels.Responses;
using LinkPara.Cache;
using LinkPara.Emoney.Application.Features.OpenBankingOperations.Queries.GetCards;
using LinkPara.Emoney.Application.Features.OpenBankingOperations.Queries.GetCardDetail;
using LinkPara.Emoney.Application.Features.OpenBankingOperations.Queries.GetCardTransactions;
using LinkPara.Emoney.Application.Features.OpenBankingOperations.Commands.CreateFuturePaymentOrderConsent;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.HttpProviders.Emoney.Enums;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using Microsoft.EntityFrameworkCore;
using LinkPara.Emoney.Application.Features.OpenBankingOperations.Commands.TriggerFuturePaymentOrder;
using Microsoft.AspNetCore.Mvc;
using LinkPara.Emoney.Application.Features.OpenBankingOperations.Commands.CancelFuturePaymentOrder;
using LinkPara.Emoney.Application.Features.OpenBankingOperations.Commands.CreateStandingPaymentOrderConsent;

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
    private readonly IGenericRepository<Account> _accountRepository;
    private readonly IServiceProvider _serviceProvider;

    public OpenBankingOperationsService(
        HttpClient client,
        IHttpContextAccessor httpContextAccessor,
        ILogger<AccountService> logger,
        IMapper mapper,
        IBus bus,
        ISecretService secretService,
        ICacheService cacheService,
        IGenericRepository<Account> accountRepository,
        IServiceProvider serviceProvider)
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
        _accountRepository = accountRepository;
        _serviceProvider = serviceProvider;
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
        _client.DefaultRequestHeaders.Add("x-app-user-id", command.AppUserId);
        _client.DefaultRequestHeaders.Add("x-request-id", Guid.NewGuid().ToString());

        var Identity = await _accountRepository
            .GetAll()
            .Where(x => x.IdentityNumber == command.AppUserId
                     && x.RecordStatus == RecordStatus.Active
                     && x.AccountType == AccountType.Individual)
            .Select(x => new AccountConsentIdentityInfo
            {
                OhkTur = "B",
                KmlkTur = "K",
                KmlkVrs = x.IdentityNumber,
                Unv = x.Name
            })
            .FirstOrDefaultAsync();

        if (Identity is null)
        {
            throw new NotFoundException(nameof(Account.IdentityNumber), command.AppUserId);
        }

        var request = new CreateAccountConsentRequest()
        {
            YonTipi = command.ForwardType == YosForwardType.Mobil ? "M" : "W",
            ErisimIzniSonTrh = command.AccessExpireDate.ToString("yyyy-MM-ddTHH:mm:ss"),
            IznTur = command.PermissionTypes,
            DrmKod = Guid.NewGuid().ToString(),
            Kmlk = Identity
        };

        string logReq = Newtonsoft.Json.JsonConvert.SerializeObject(request);
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
            YetTip = "yet_kod",
            YetKod = query.AccessCode,
        };

        string logReq = Newtonsoft.Json.JsonConvert.SerializeObject(request);
        var accessTokenResponse = await YosServicePostAsync<YosServiceResultDto>("ohvps/gkd/s1.0/erisim-belirteci", request, logReq, "GetHhsAccessToken");

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
        _client.DefaultRequestHeaders.Add("x-app-user-id", query.AppUserId);

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
        _client.DefaultRequestHeaders.Add("x-app-user-id", query.AppUserId);
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
        _client.DefaultRequestHeaders.Add("x-app-user-id", query.AppUserId);
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
        _client.DefaultRequestHeaders.Add("x-app-user-id", query.AppUserId);
        _client.DefaultRequestHeaders.Add("x-aspsp-code", query.HhsCode);

        string logReq = Newtonsoft.Json.JsonConvert.SerializeObject(query);
        var consentedAccountBalancesResult = await YosServiceGetAsync<ConsentedAccountBalancesResultDto>($"ohvps/hbh/s1.0/bakiye", logReq, "GetConsentedAccountBalanceList");

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
        _client.DefaultRequestHeaders.Add("x-app-user-id", query.AppUserId);
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
        _client.DefaultRequestHeaders.Add("x-app-user-id", query.AppUserId);
        _client.DefaultRequestHeaders.Add("x-aspsp-code", query.HhsCode);
        
        string logReq = Newtonsoft.Json.JsonConvert.SerializeObject(query);


        var tz = TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time");
        var parameters = new Dictionary<string, object>
        {
            ["hesapIslemBslTrh"] = TimeZoneInfo.ConvertTime(query.HesapIslemBslTrh, tz),
            ["hesapIslemBtsTrh"] = TimeZoneInfo.ConvertTime(query.HesapIslemBtsTrh, tz),
            ["minIslTtr"] = query.MinIslTtr,
            ["mksIslTtr"] = query.MksIslTtr,
            ["brcAlc"] = query.BrcAlc,
            ["syfNo"] = query.SyfNo,
            ["srlmKrtr"] = query.SrlmKrtr,
            ["srlmYon"] = query.SrlmYon,
            ["syfKytSayi"] = query.SyfKytSayi
        };

        var queryString = ToQueryString(parameters);

        queryString = $"ohvps/hbh/s1.0/hesaplar/{query.AccountReference}/islemler?{queryString}";        
        var consentedAccountActivitiesResult = await YosServiceGetAsync<ConsentedAccountActivitiesResultDto>(queryString, logReq, "GetConsentedAccountActivities");

        return consentedAccountActivitiesResult;
    }

    public static string ToQueryString(Dictionary<string, object> parameters)
    {
        var queryParts = new List<string>();

        foreach (var param in parameters)
        {
            if (param.Value == null)
                continue;

            var value = param.Value switch
            {
                DateTime dt => dt.ToString("yyyy-MM-ddTHH:mm:sszzz"),
                _ => param.Value.ToString()
            };

            queryParts.Add($"{param.Key}={value}");
        }

        return string.Join("&", queryParts);
    }

    public async Task<PaymentOrderConsentDetailDto> CreatePaymentConsentAsync(CreatePaymentConsentCommand command)
    {
        var token = await GetAirapiAccessTokenAsync();
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", token);
        _client.DefaultRequestHeaders.Add("PSU-Initiated", "E");
        _client.DefaultRequestHeaders.Add("x-request-id", Guid.NewGuid().ToString());
        _client.DefaultRequestHeaders.Add("x-app-user-id", command.AppUserId);
        _client.DefaultRequestHeaders.Add("x-aspsp-code", command.HhsCode);

        var request = _mapper.Map<CreatePaymentConsentRequest>(command);
        request.YonTipi = command.YonTipi == YosForwardType.Mobil ? "M" : "W";
        request.OdmAyr.RefBlg = Guid.NewGuid().ToString();

        string logReq = Newtonsoft.Json.JsonConvert.SerializeObject(request);
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
        _client.DefaultRequestHeaders.Add("x-app-user-id", query.AppUserId);
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
        _client.DefaultRequestHeaders.Add("x-app-user-id", command.AppUserId);
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
        _client.DefaultRequestHeaders.Add("x-app-user-id", query.AppUserId);
        _client.DefaultRequestHeaders.Add("x-aspsp-code", query.HhsCode);
        _client.DefaultRequestHeaders.Add("x-riza-no", query.ConsentId);
        _client.DefaultRequestHeaders.Add("isPaymentOrConsentId", "1");

        string logReq = Newtonsoft.Json.JsonConvert.SerializeObject(query);

        var paymentOrderResult = await YosServiceGetAsync<PaymentOrderDetailResultDto>($"ohvps/obh/s1.0/odeme-emri/{query.ConsentId}", logReq, "PaymentOrderDetailQuery");
        return paymentOrderResult;
    }

    public async Task<CardsResultDto> GetCardsAsync(GetCardsQuery command)
    {
        var token = await GetAirapiAccessTokenAsync();
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", token);
        _client.DefaultRequestHeaders.Add("PSU-Initiated", "E");
        _client.DefaultRequestHeaders.Add("x-request-id", Guid.NewGuid().ToString());
        _client.DefaultRequestHeaders.Add("x-app-user-id", command.ApplicationUser);
        _client.DefaultRequestHeaders.Add("x-aspsp-code", command.HhsCode);;
        _client.DefaultRequestHeaders.Add("applicationUser", command.ApplicationUser);

        string logReq = Newtonsoft.Json.JsonConvert.SerializeObject(command);

        var cardsResult = await YosServicePostAsync<CardsResultDto>("ohvps/hbh/s1.0/kartlar", new { }, logReq, "GetCards");

        return cardsResult;
    }

    public async Task<CardDetailResultDto> GetCardDetailAsync(GetCardDetailQuery command)
    {
        var token = await GetAirapiAccessTokenAsync();
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", token);
        _client.DefaultRequestHeaders.Add("PSU-Initiated", "E");
        _client.DefaultRequestHeaders.Add("x-request-id", Guid.NewGuid().ToString());
        _client.DefaultRequestHeaders.Add("x-app-user-id", command.AppUserId);
        _client.DefaultRequestHeaders.Add("x-aspsp-code", command.HhsCode);
        _client.DefaultRequestHeaders.Add("x-riza-no", command.ConsentId);

        string logReq = Newtonsoft.Json.JsonConvert.SerializeObject(command);

        var cardDetailsResult = await YosServicePostAsync<CardDetailResultDto>("ohvps/obh/s1.0/kart-detay", new { }, logReq, "GetCardDetail");

        return cardDetailsResult;
    }


    public async Task<CardTransactionsResultDto> GetCardTransactionsAsync(GetCardTransactionsQuery command)
    {
        var token = await GetAirapiAccessTokenAsync();
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", token);
        _client.DefaultRequestHeaders.Add("PSU-Initiated", "E");
        _client.DefaultRequestHeaders.Add("x-request-id", Guid.NewGuid().ToString());
        _client.DefaultRequestHeaders.Add("x-app-user-id", command.AppUserId);
        _client.DefaultRequestHeaders.Add("x-aspsp-code", command.HhsCode);

        string logReq = Newtonsoft.Json.JsonConvert.SerializeObject(command);

        var parameters = new Dictionary<string, object>
        {
            ["consentId"] = command.ConsentId,
            ["cardRefNo"] = command.CardRefNo,
            ["periodValue"] = command.PeriodValue,
            ["statementType"] = command.StatementType,
            ["pageRecordCount"] = command.PageRecordCount,
            ["pageNo"] = command.PageNo,
            ["debtOrCredit"] = command.DebtOrCredit,
            ["orderType"] = command.OrderType
        };

        var cardTransactionsResult = await YosServicePostAsync<CardTransactionsResultDto>("ohvps/hbh/s1.0/kartlar/kart-hareketleri", parameters, logReq, "GetCardTransactions");

        return cardTransactionsResult;
    }

    public async Task<FuturePaymentOrderConsentResultDto> CreateFuturePaymentOrderConsentAsync(CreateFuturePaymentOrderConsentCommand command)
    {
        var token = await GetAirapiAccessTokenAsync();
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", token);
        _client.DefaultRequestHeaders.Add("PSU-Initiated", "E");
        _client.DefaultRequestHeaders.Add("x-request-id", Guid.NewGuid().ToString());
        _client.DefaultRequestHeaders.Add("x-app-user-id", command.AppUserId);
        _client.DefaultRequestHeaders.Add("x-aspsp-code", command.HhsCode);
        _client.DefaultRequestHeaders.Add("x-riza-no", command.ConsentId);

        string logReq = Newtonsoft.Json.JsonConvert.SerializeObject(command);

        var futurePaymentOrderConsentResult = await YosServicePostAsync<FuturePaymentOrderConsentResultDto>("ohvps/obh/s1.0/ileri-tarihli-odeme-emri-rizasi", new { }, logReq, "CreateFuturePaymentOrderConsent");

        return futurePaymentOrderConsentResult;
    }

    public async Task<TriggerFuturePaymentOrderResultDto> TriggerFuturePaymentOrderAsync(TriggerFuturePaymentOrderCommand command)
    {
        var token = await GetAirapiAccessTokenAsync();
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", token);
        _client.DefaultRequestHeaders.Add("PSU-Initiated", "E");
        _client.DefaultRequestHeaders.Add("x-request-id", Guid.NewGuid().ToString());
        _client.DefaultRequestHeaders.Add("x-app-user-id", command.AppUserId);
        _client.DefaultRequestHeaders.Add("x-aspsp-code", command.HhsCode);
        _client.DefaultRequestHeaders.Add("x-riza-no", command.ConsentId);

        string logReq = Newtonsoft.Json.JsonConvert.SerializeObject(command);

        var triggerPaymentOrderResult = await YosServicePostAsync<TriggerFuturePaymentOrderResultDto>("ohvps/obh/s1.0/ileri-tarihli-odeme-emri", new { }, logReq, "CreateFuturePaymentOrderConsent");

        return triggerPaymentOrderResult;
    }

    public async Task<GetFuturePaymentOrderListResultDto> GetFuturePaymentOrderListAsync([FromBody] GetFuturePaymentOrderListQuery query)
    {
        var token = await GetAirapiAccessTokenAsync();
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", token);
        _client.DefaultRequestHeaders.Add("PSU-Initiated", "E");
        _client.DefaultRequestHeaders.Add("x-request-id", Guid.NewGuid().ToString());

        string logReq = Newtonsoft.Json.JsonConvert.SerializeObject(query);

        var futurePaymentOrderList = await YosServicePostAsync<GetFuturePaymentOrderListResultDto>("ohvps/obh/s1.0/ileri-tarihli-odeme-emri-listesi", new { }, logReq, "CreateFuturePaymentOrderConsent");

        return futurePaymentOrderList;
    }

    public async Task<CancelFuturePaymentOrderResultDto> CancelFuturePaymentOrderAsync([FromBody] CancelFuturePaymentOrderCommand command)
    {
        var token = await GetAirapiAccessTokenAsync();
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", token);
        _client.DefaultRequestHeaders.Add("PSU-Initiated", "E");
        _client.DefaultRequestHeaders.Add("x-request-id", Guid.NewGuid().ToString());
        _client.DefaultRequestHeaders.Add("x-aspsp-code", command.HhsCode);

        string logReq = Newtonsoft.Json.JsonConvert.SerializeObject(command);

        var cancelPaymentOrderResult = await YosServicePostAsync<CancelFuturePaymentOrderResultDto>("ohvps/obh/s1.0/ileri-tarihli-odeme-emri-iptal", new { }, logReq, "CreateFuturePaymentOrderConsent");

        return cancelPaymentOrderResult;
    }

    public async Task<StandingPaymentOrderConsentResultDto> CreateStandingPaymentOrderConsentAsync(CreateStandingPaymentOrderConsentCommand command)
    {
        var token = await GetAirapiAccessTokenAsync();
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", token);
        _client.DefaultRequestHeaders.Add("PSU-Initiated", "E");
        _client.DefaultRequestHeaders.Add("x-request-id", Guid.NewGuid().ToString());
        _client.DefaultRequestHeaders.Add("x-app-user-id", command.AppUserId);
        _client.DefaultRequestHeaders.Add("x-aspsp-code", command.HhsCode);
        _client.DefaultRequestHeaders.Add("x-riza-no", command.ConsentId);

        string logReq = Newtonsoft.Json.JsonConvert.SerializeObject(command);

        var recurringPaymentOrderConsentResult = await YosServicePostAsync<StandingPaymentOrderConsentResultDto>("ohvps/obh/s1.0/duzenli-odeme-emri-rizasi", new { }, logReq, "CreateFuturePaymentOrderConsent");

        return recurringPaymentOrderConsentResult;
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
