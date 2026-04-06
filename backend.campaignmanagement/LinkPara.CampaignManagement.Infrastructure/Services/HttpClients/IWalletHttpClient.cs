using LinkPara.CampaignManagement.Application.Commons.Interfaces;
using LinkPara.CampaignManagement.Application.Commons.Interfaces.HttpClients;
using LinkPara.CampaignManagement.Application.Commons.IWalletConfigurations;
using LinkPara.CampaignManagement.Application.Commons.Models.Requests;
using LinkPara.CampaignManagement.Application.Commons.Models.Responses;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Text.Json;
using LinkPara.CampaignManagement.Domain.Entities;
using LinkPara.CampaignManagement.Domain.Enums;
using LinkPara.HttpProviders.Vault;

namespace LinkPara.CampaignManagement.Infrastructure.Services.HttpClients;

public class IWalletHttpClient : IIWalletHttpClient
{

    private readonly JsonSerializerOptions jsonSerializerOption = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString
    };

    private readonly HttpClient _client;
    private readonly IAuthorizationService _authorizationService;
    private readonly ILogger<IWalletHttpClient> _logger;
    private readonly IVaultClient _vaultClient;

    public IWalletHttpClient(HttpClient client,
        IAuthorizationService authorizationService,
        ILogger<IWalletHttpClient> logger,
        IVaultClient vaultClient)
    {
        _client = client;
        _authorizationService = authorizationService;
        _logger = logger;
        _vaultClient = vaultClient;
    }

    public async Task<List<Campaign>> GetCampaignsAsync()
    {
        string token = await GetTokenAsync();

        if (_client.DefaultRequestHeaders.Contains("Authorization"))
        {
            _client.DefaultRequestHeaders.Remove("Authorization");
        }

        _client.DefaultRequestHeaders.Add("Authorization", $"Token token={token}");

        var code = _vaultClient.GetSecretValue<string>("CampaignManagementSecrets", "IWalletSettings", "Code");

        var response = await _client.GetAsync($"api/v2/campaigns?campaign_type=s&code={code}&locale=tr");

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Campaigns Response is not successful. StatusCode: {StatusCode}, Content: {Content}", response.StatusCode, await response.Content.ReadAsStringAsync());
            throw new InvalidOperationException();
        }

        string resposeData = await HandleServiceResponseAsync(response);

        var result = JsonSerializer.Deserialize<CampaignResponse>(resposeData, jsonSerializerOption);

        return result.Data;
    }

    private async Task<string> HandleServiceResponseAsync(HttpResponseMessage response)
    {
        var resposeData = await response.Content.ReadAsStringAsync();

        var baseMessage = JsonSerializer.Deserialize<BaseResponse>(resposeData, jsonSerializerOption);

        if (baseMessage.code != "200")
        {
            _logger.LogError("Code: {Code} Message: {ResposeData}", baseMessage.code, resposeData);
            throw new InvalidOperationException(resposeData);
        }

        return resposeData;
    }

    private async Task<string> GetTokenAsync()
    {
        string token = await _authorizationService.GetActiveTokenAsync();

        if (string.IsNullOrWhiteSpace(token))
        {
            var loginResult = await LoginAsync();
            token = await _authorizationService.RefreshTokenAsync(loginResult);
        }
        return token;
    }

    private async Task<LoginResponse> LoginAsync()
    {
        var iWalletSettings = _vaultClient.GetSecretValue<IWalletSettings>("CampaignManagementSecrets", "IWalletSettings");
        var loginRequest = new LoginRequest { username = iWalletSettings.Username, password = iWalletSettings.Password };

        var response = await _client.PostAsJsonAsync<LoginRequest>("api/v1/login", loginRequest);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Login Response is not successful. StatusCode: {StatusCode}, Content: {Content}", response.StatusCode, await response.Content.ReadAsStringAsync());
            throw new InvalidOperationException();
        }

        string resposeData = await HandleServiceResponseAsync(response);

        var result = JsonSerializer.Deserialize<LoginResponse>(resposeData, jsonSerializerOption);

        return result;
    }

    public async Task<List<Agreement>> GetAgreementsAsync()
    {
        string token = await GetTokenAsync();

        if (_client.DefaultRequestHeaders.Contains("Authorization"))
        {
            _client.DefaultRequestHeaders.Remove("Authorization");
        }

        _client.DefaultRequestHeaders.Add("Authorization", $"Token token={token}");

        var response = await _client.GetAsync("api/v2/agreements");

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Aggrements Response is not successful. StatusCode: {StatusCode}, Content: {Content}", response.StatusCode, await response.Content.ReadAsStringAsync());
            throw new InvalidOperationException();
        }

        string resposeData = await HandleServiceResponseAsync(response);

        var result = JsonSerializer.Deserialize<AgreementResponse>(resposeData, jsonSerializerOption);

        return result.Data;
    }

    public async Task<List<City>> GetCitiesAsync()
    {
        string token = await GetTokenAsync();

        if (_client.DefaultRequestHeaders.Contains("Authorization"))
        {
            _client.DefaultRequestHeaders.Remove("Authorization");
        }

        _client.DefaultRequestHeaders.Add("Authorization", $"Token token={token}");

        var response = await _client.GetAsync("api/v2/cities");

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Cities Response is not successful. StatusCode: {StatusCode}, Content: {Content}", response.StatusCode, await response.Content.ReadAsStringAsync());
            throw new InvalidOperationException();
        }

        string resposeData = await HandleServiceResponseAsync(response);

        var result = JsonSerializer.Deserialize<CityResponse>(resposeData, jsonSerializerOption);

        return result.Data;
    }

    public async Task<List<Town>> GetTownsAsync(int cityId)
    {
        string token = await GetTokenAsync();

        if (_client.DefaultRequestHeaders.Contains("Authorization"))
        {
            _client.DefaultRequestHeaders.Remove("Authorization");
        }

        _client.DefaultRequestHeaders.Add("Authorization", $"Token token={token}");

        var response = await _client.GetAsync($"api/v2/cities/{cityId}/towns");

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Towns Response is not successful. StatusCode: {StatusCode}, Content: {Content}", response.StatusCode, await response.Content.ReadAsStringAsync());
            throw new InvalidOperationException();
        }

        string resposeData = await HandleServiceResponseAsync(response);

        var result = JsonSerializer.Deserialize<TownResponse>(resposeData, jsonSerializerOption);

        return result.Data;
    }

    public async Task<IWalletCard> CreateCardAsync(IWalletCard card)
    {
        try
        {
            var cardServiceRequest = PrepareCardServiceRequest(card);

            string token = await GetTokenAsync();

            if (_client.DefaultRequestHeaders.Contains("Authorization"))
            {
                _client.DefaultRequestHeaders.Remove("Authorization");
            }

            _client.DefaultRequestHeaders.Add("Authorization", $"Token token={token}");

            var response = await _client.PostAsJsonAsync<CardApplicationRequest>("api/v2/cards", cardServiceRequest);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("cards Response is not successful. StatusCode: {StatusCode}, Content: {Content}", response.StatusCode, await response.Content.ReadAsStringAsync());
                throw new InvalidOperationException();
            }

            string resposeData = await HandleServiceResponseAsync(response);

            var result = JsonSerializer.Deserialize<CardResponse>(resposeData, jsonSerializerOption);

            card.CardNumber = result.Data.cc_number;
            card.CustomerBranchId = result.Data.customer_branch_id;
            card.CardId = result.Data.id;
            card.CustomerId = result.Data.customer_id;

            return card;
        }
        catch (Exception ex)
        {
            _logger.LogError("CreateCardAsync Error: {Ex}", ex);
            card.CardNumber = string.Empty;
            card.ErrorMessage = ex.Message;
            return card;
        }
       
    }

    private CardApplicationRequest PrepareCardServiceRequest(IWalletCard request)
    {
        var iWalletSettings = _vaultClient.GetSecretValue<IWalletSettings>("CampaignManagementSecrets", "IWalletSettings");
        var walletId = iWalletSettings.WalletId;
        var code = iWalletSettings.Code;
        var branchName = iWalletSettings.BranchName;

        return new CardApplicationRequest
        {
            card = new CardRequest
            {
                address = request.AddressDetail,
                branch_name = branchName,
                city_id = request.CityId,
                code = code,
                commercial_electronic_communication_aggrement = request.IsApprovedCommercialElectronicCommunicationAggrement,
                email = request.Email,
                ext_wallet_id = request.WalletNumber,
                gsm = request.PhoneNumber.Substring(3, request.PhoneNumber.Length - 3),
                individual_framework_agreement = request.IsApprovedIndividualFrameworkAgreement,
                kvkk_agreement = request.IsApprovedKvkkAgreement,
                name_surname = request.FullName,
                preliminary_information_agreement = request.IsApprovedPreliminaryInformationAgreement,
                tc_no = request.IdentityNumber,
                town_id = request.TownId,
                utype = request.UserType == UserType.Individual ? "i" : "c",
                wallet_id = walletId
            }
        };
    }

    public async Task<QrCodeResponse> GenerateQrCodeAsync(int cardId)
    {
        var timeout = _vaultClient.GetSecretValue<string>("CampaignManagementSecrets", "IWalletSettings", "QrCodeTimeoutInSeconds");
        var qrRequest = new QrCodeRequest
        {
            time_out = int.Parse(timeout)
        };

        string token = await GetTokenAsync();

        if (_client.DefaultRequestHeaders.Contains("Authorization"))
        {
            _client.DefaultRequestHeaders.Remove("Authorization");
        }

        _client.DefaultRequestHeaders.Add("Authorization", $"Token token={token}");

        var response = await _client.PostAsJsonAsync<QrCodeRequest>($"api/v1/cards/{cardId}/create_qr_code", qrRequest);

        string resposeData = await HandleServiceResponseAsync(response);

        var result = JsonSerializer.Deserialize<QrCodeResponse>(resposeData, jsonSerializerOption);

        return result;
    }
}
