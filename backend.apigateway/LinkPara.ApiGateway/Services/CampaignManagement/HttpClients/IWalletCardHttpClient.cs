using LinkPara.ApiGateway.Commons.Extensions;
using LinkPara.ApiGateway.Commons.Helpers;
using LinkPara.ApiGateway.Services.CampaignManagement.Models.Requests;
using LinkPara.ApiGateway.Services.CampaignManagement.Models.Responses;
using LinkPara.ApiGateway.Services.Emoney.HttpClients;
using LinkPara.ApiGateway.Services.Emoney.Models.Requests;
using LinkPara.ApiGateway.Services.Identity.HttpClients;
using LinkPara.ApiGateway.Services.Identity.Models.Requests;
using LinkPara.HttpProviders.CustomerManagement;
using LinkPara.SharedModels.Exceptions;
using System.Text.Json;

namespace LinkPara.ApiGateway.Services.CampaignManagement.HttpClients;

public class IWalletCardHttpClient : HttpClientBase, IIWalletCardHttpClient
{
    private readonly IWalletHttpClient _emoneyWalletHttpClient;
    private readonly IUserHttpClient _userHttpClient;
    private readonly IServiceRequestConverter _serviceRequestConverter;
    private readonly IEmoneyAccountHttpClient _emMoneyAccountHttpClient;
    private readonly ICustomerService _customerService;

    public IWalletCardHttpClient(HttpClient client,
        IHttpContextAccessor httpContextAccessor,
        IWalletHttpClient emoneyWalletHttpClient,
        IServiceRequestConverter serviceRequestConverter,
        IUserHttpClient userHttpClient,
        IEmoneyAccountHttpClient emMoneyAccountHttpClient,
        ICustomerService customerService)
        : base(client, httpContextAccessor)
    {
        _emoneyWalletHttpClient = emoneyWalletHttpClient;
        _serviceRequestConverter = serviceRequestConverter;
        _userHttpClient = userHttpClient;
        _emMoneyAccountHttpClient = emMoneyAccountHttpClient;
        _customerService = customerService;
    }

    public async Task<IWalletQrCodeResponse> CreateCardAsync(IWalletCreateCardRequest request)
    {
        var clientRequest = _serviceRequestConverter.Convert<IWalletCreateCardRequest, IWalletCreateCardServiceRequest>(request);
        
        var user = await _userHttpClient.GetUserAsync(new GetUserRequest { });

        if(user is null)
        {
            throw new NotFoundException("user");
        }

        var wallets = await _emoneyWalletHttpClient.GetUserWalletsAsync(new GetUserWalletsFilterRequest { });
        var mainWallet = wallets.FirstOrDefault(s => s.IsMainWallet);

        if (mainWallet is null)
        {
            throw new NotFoundException("wallet");
        }

        clientRequest.WalletNumber = mainWallet.WalletNumber;
        clientRequest.FullName = user.FirstName + " " + user.LastName;
        clientRequest.IdentityNumber = user.IdentityNumber;
        clientRequest.Email = user.Email;
        clientRequest.PhoneNumber = $"{user.PhoneCode}{user.PhoneNumber}";

        var accountUser = await _emMoneyAccountHttpClient.GetAccountByUserIdAsync(user.Id);

        if (accountUser is null)
        {
            throw new NotFoundException(nameof(accountUser), user.Id);
        }

        var customer = await _customerService.GetCustomerAsync(accountUser.CustomerId);

        if (customer is null)
        {
            throw new NotFoundException(nameof(customer), accountUser.CustomerId);
        }

        var customerAddress = customer.CustomerAddresses.FirstOrDefault(x => x.Primary);

        if(!int.TryParse(customerAddress.CityIso2,out int cityId))
        {
            throw new InvalidCastException(nameof(customerAddress.CityIso2));
        }

        clientRequest.AddressDetail = "TR";
        clientRequest.CityId = cityId;
        clientRequest.TownId = customerAddress.DistrictId;

        var response = await PostAsJsonAsync<IWalletCreateCardServiceRequest>($"v1/IWalletCards", clientRequest);

        var responseString = await response.Content.ReadAsStringAsync();
        var qrCode = JsonSerializer.Deserialize<IWalletQrCodeResponse>(responseString, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return qrCode ?? throw new InvalidOperationException();
    }

    public async Task<IWalletCardResponse> GetUserIWalletCardAsync(GetUserIWalletCardsFilterRequest getUserIWalletCardsFilterRequest)
    {
        var clientRequest = _serviceRequestConverter.Convert<GetUserIWalletCardsFilterRequest, GetUserIWalletCardsFilterServiceRequest>(getUserIWalletCardsFilterRequest);
        var queryString = clientRequest.GetQueryString();

        var response = await GetAsync($"v1/IWalletCards" + queryString);
        var responseString = await response.Content.ReadAsStringAsync();
        var card = JsonSerializer.Deserialize<IWalletCardResponse>(responseString, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return card ?? throw new InvalidOperationException();
    }
}
