using AutoMapper;
using LinkPara.Emoney.Application.Commons.Enums;
using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Application.Commons.Models.ConsentModels.Requests;
using LinkPara.Emoney.Application.Commons.Models.ConsentModels.Responses;
using LinkPara.Emoney.Application.Features.ConsentOperations;
using LinkPara.Emoney.Application.Features.ConsentOperations.Commands.CancelConsent;
using LinkPara.Emoney.Application.Features.ConsentOperations.Commands.UpdateConsent;
using LinkPara.Emoney.Application.Features.ConsentOperations.Queries.GetActiveConsentList;
using LinkPara.Emoney.Application.Features.ConsentOperations.Queries.GetConsentDetail;
using LinkPara.Emoney.Application.Features.ConsentOperations.Queries.GetWaitingApprovalConsents;
using LinkPara.Emoney.Domain.Enums;
using LinkPara.HttpProviders;
using LinkPara.HttpProviders.Utility;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Logging;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Logging.Enums;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;

namespace LinkPara.Emoney.Infrastructure.Services;

public class ConsentOperationsService : HttpClientBase, IConsentOperationsService
{
    private readonly HttpClient _client;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AccountService> _logger;
    private readonly IMapper _mapper;
    private readonly IBus _bus;
    private readonly ISecretService _secretService;

    public ConsentOperationsService(
        HttpClient client,
        IHttpContextAccessor httpContextAccessor,
        ILogger<AccountService> logger,
        IMapper mapper,
        IBus bus,
        ISecretService secretService)
        : base(client, httpContextAccessor)
    {
        _client = client;
        _client.BaseAddress = new Uri(secretService.OpenBankingHhsSettings.OpenBankingUrl);
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
        _mapper = mapper;
        _bus = bus;
        _secretService = secretService;
    }

    private async Task<TokenResponse> GetTokenAsync()
    {
        var client = new HttpClient()
        {
            BaseAddress = new Uri(_secretService.OpenBankingHhsSettings.Authority)
        };
        var collection = new List<KeyValuePair<string, string>>(){
                new("client_id", _secretService.OpenBankingHhsSettings.ClientId),
                new("client_secret", _secretService.OpenBankingHhsSettings.ClientSecret),
                new("Scope", _secretService.OpenBankingHhsSettings.Scope),
                new("grant_type", _secretService.OpenBankingHhsSettings.GrantType)
        };
        var content = new FormUrlEncodedContent(collection);

        var response = await client.PostAsync("connect/token", content);

        var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>();

        return tokenResponse;
    }

    public async Task<List<ConsentDto>> GetActiveConsentListAsync(GetActiveConsentListQuery query)
    {
        var token = await GetTokenAsync();
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", $"{token.Token_Type} {token.Access_Token}");
        _client.DefaultRequestHeaders.Add("x-customer-id", query.AccountId);
        _client.DefaultRequestHeaders.Add("x-consent-type", query.ConsentType == ConsentType.AccountInfo ? "A" : "P");
        _client.DefaultRequestHeaders.Add("x-status", "1");

        string logReq = Newtonsoft.Json.JsonConvert.SerializeObject(query);

        var responseString = await ConsentPostAsync("v1/tcmb/consent/list", new { }, logReq, "ActiveConsent");

        var activeConsentResponse = JsonSerializer.Deserialize<List<ConsentDto>>(responseString,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        foreach (var consent in activeConsentResponse)
        {
            consent.LastAccessDateValue = DateTime.Parse(consent.LastAccessDate);
            consent.CreateDateValue = DateTime.Parse(consent.CreateDate);
        }
        return activeConsentResponse;
    }

    public async Task<CancelConsentResultDto> CancelConsentAsync(CancelConsentCommand command)
    {
        var token = await GetTokenAsync();
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", $"{token.Token_Type} {token.Access_Token}");
        var request = _mapper.Map<CancelConsentServiceRequest>(command);
        request.ConsentType = command.ConsentTypeValue == ConsentType.PaymentOrder ? "P" : "A";


        string logReq = Newtonsoft.Json.JsonConvert.SerializeObject(command);

        var responseString = await ConsentPostAsync("v1/tcmb/consent/delete", request, logReq, "CancelConsent");

        var cancelResponse = JsonSerializer.Deserialize<CancelConsentResultDto>(responseString,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        cancelResponse.IsSuccess = cancelResponse.HttpCode == 1;

        return cancelResponse;
    }

    public async Task<GetWaitingApprovalConsentResponse> GetWaitingApprovalConsentsAsync(GetWaitingApprovalConsentQuery query)
    {
        var waitingApprovalConsentResponse = new GetWaitingApprovalConsentResponse();

        var token = await GetTokenAsync();
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", $"{token.Token_Type} {token.Access_Token}");

        var queryString = GetQueryString.CreateUrlWithParams("v1/tcmb/consent/GetApproveWaitingConsent", query);

        string logReq = Newtonsoft.Json.JsonConvert.SerializeObject(query);
        var responseString = await ConsentGetAsync(queryString, logReq, "WaitingApprovalConsent");

        var waitingApprovalList = JsonSerializer.Deserialize<List<WaitingApprovalConsentDto>>(responseString,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        waitingApprovalConsentResponse.Value = waitingApprovalList;

        return waitingApprovalConsentResponse;
    }
    public async Task<GetConsentDetailResponse> GetConsentDetailAsync(GetConsentDetailQuery query)
    {
        var token = await GetTokenAsync();
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", $"{token.Token_Type} {token.Access_Token}");

        var queryString = GetQueryString.CreateUrlWithParams("v1/tcmb/consent/getConsentData", query);

        string logReq = Newtonsoft.Json.JsonConvert.SerializeObject(query);
        var responseString = await ConsentGetAsync(queryString, logReq, "ConsentDetail");

        var consentDetailResponse = JsonSerializer.Deserialize<GetConsentDetailResponse>(responseString,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        consentDetailResponse.ConsentTypeValue = consentDetailResponse.ConsentType == 1 ? ConsentType.AccountInfo : ConsentType.PaymentOrder;
        consentDetailResponse.CustomerTypeValue = consentDetailResponse.CustomerType == "B" ? WalletType.Individual : WalletType.Corporate;

        return consentDetailResponse;
    }

    public async Task<UpdateConsentResultDto> UpdateConsentAsync(UpdateConsentCommand command)
    {
        var token = await GetTokenAsync();
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", $"{token.Token_Type} {token.Access_Token}");

        command.SelectedAccountResponse = Newtonsoft.Json.JsonConvert.SerializeObject(command.Accounts);
        var request = _mapper.Map<UpdateConsentServiceRequest>(command);
        request.ConsentType = command.ConsentTypeValue == ConsentType.PaymentOrder ? "P" : "A";

        string logReq = Newtonsoft.Json.JsonConvert.SerializeObject(command);
        var responseString = await ConsentPostAsync("v1/tcmb/consent/update", request, logReq, "UpdateConsent");

        var updateResponse = JsonSerializer.Deserialize<UpdateConsentResultDto>(responseString,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return updateResponse;
    }


    private async Task<string> ConsentGetAsync(string queryString, string jsonData, string processName)
    {
        try
        {
            var correlationId = Guid.NewGuid();

            await SendIntegrationRequest(jsonData, processName, correlationId, IntegrationLogDataType.Json);

            var response = await _client.GetAsync(queryString);

            await SendIntegrationResponse(processName, response, correlationId);

            var responseString = await response.Content.ReadAsStringAsync();

            return responseString;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Consent Operation Get Error: {ex.Message}");
            throw;
        }

    }
    private async Task<string> ConsentPostAsync(string url, object data, string jsonData, string processName)
    {
        try
        {
            var correlationId = Guid.NewGuid();

            await SendIntegrationRequest(jsonData, processName, correlationId, IntegrationLogDataType.Json);

            var response = await _client.PostAsJsonAsync(url, data);

            await SendIntegrationResponse(processName, response, correlationId);

            var responseString = await response.Content.ReadAsStringAsync();

            return responseString;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Consent Operation Post Error: {ex.Message}");
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
            _logger.LogError($"Send Consent Request Data To Integration Log Error: {exception}");
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
            _logger.LogError($"Send Consent Response Data To Integration Log Error: {exception}");
        }

    }

    public async Task<List<GetControlBalanceConsentResponse>> GetConsentsForChangeBalanceControl()
    {
        var token = await GetTokenAsync();
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", $"{token.Token_Type} {token.Access_Token}");

        var request = new GetControlBalanceConsentRequest { TenantId = _secretService.OpenBankingHhsSettings.TenantId };

        var response = await _client.PostAsJsonAsync("v1/tcmb/consent/GetConsentsForChangeBalanceControl", request);

        return await response.Content.ReadFromJsonAsync<List<GetControlBalanceConsentResponse>>();

    }
}
