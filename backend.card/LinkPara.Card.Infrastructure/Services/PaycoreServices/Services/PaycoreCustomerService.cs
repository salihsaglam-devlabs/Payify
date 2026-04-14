using LinkPara.Card.Application.Commons.Interfaces;
using LinkPara.Card.Application.Commons.Models.PaycoreModels;
using LinkPara.Card.Application.Commons.Models.PaycoreModels.CustomerModels;
using LinkPara.Card.Application.Features.PaycoreServices.CustomerServices.Commands.CreateCustomer;
using LinkPara.Card.Application.Features.PaycoreServices.CustomerServices.Commands.UpdateCustomer;
using LinkPara.Card.Application.Features.PaycoreServices.CustomerServices.Commands.UpdateCustomerAddress;
using LinkPara.Card.Application.Features.PaycoreServices.CustomerServices.Commands.UpdateCustomerCommunication;
using LinkPara.Card.Application.Features.PaycoreServices.CustomerServices.Commands.UpdateCustomerLimit;
using LinkPara.Card.Application.Features.PaycoreServices.CustomerServices.Queries.GetCustomerCards;
using LinkPara.Card.Application.Features.PaycoreServices.CustomerServices.Queries.GetCustomerInformation;
using LinkPara.Card.Application.Features.PaycoreServices.CustomerServices.Queries.GetCustomerLimitInfo;
using LinkPara.Card.Domain.Constants;
using LinkPara.Card.Domain.Entities;
using LinkPara.Card.Infrastructure.Services.PaycoreServices.Models;
using LinkPara.Card.Infrastructure.Services.PaycoreServices.Models.CustomerModels.Requests;
using LinkPara.ContextProvider;
using LinkPara.HttpProviders.Vault;
using LinkPara.Security;
using LinkPara.SharedModels.Persistence;
using Microsoft.Extensions.Configuration;
using System.Globalization;

namespace LinkPara.Card.Infrastructure.Services.PaycoreServices.Services;
public class PaycoreCustomerService : IPaycoreCustomerService
{
    private readonly PaycoreClientService _clientService;
    private readonly IConfiguration _configuration;
    private readonly IVaultClient _vaultClient;
    private readonly PaycoreSettings _paycoreSettings;
    private readonly ISecureRandomGenerator _randomGenerator;
    private readonly IGenericRepository<CustomerWalletCard> _customerWalletCard;
    private readonly IContextProvider _contextProvider;
    public PaycoreCustomerService(
        PaycoreClientService clientService,
        IConfiguration configuration,
        IVaultClient vaultClient,
        ISecureRandomGenerator randomGenerator,
        IGenericRepository<CustomerWalletCard> customerWalletCard,
        IContextProvider contextProvider)
    {
        _clientService = clientService;
        _configuration = configuration;
        _vaultClient = vaultClient;
        _paycoreSettings = new PaycoreSettings();
        _configuration.GetSection(nameof(PaycoreSettings)).Bind(_paycoreSettings);
        _paycoreSettings.VaultSettings = _vaultClient.GetSecretValue<PaycoreVaultSettings>("CardSecrets", "PaycoreSettings");
        _randomGenerator = randomGenerator;
        _customerWalletCard = customerWalletCard;
        _contextProvider = contextProvider;
    }
    public async Task<PaycoreResponse> CreateCustomerAsync(CreateCustomerCommand command)
    {
        var customerWallets = _customerWalletCard
            .GetAll()
            .Where(x => x.WalletNumber == command.WalletNumber &&
                        x.RecordStatus == RecordStatus.Active);

        if (customerWallets.Any())
        {
            var customerWallet = customerWallets.FirstOrDefault(x => x.ProductCode == command.ProductCode);

            if (customerWallet is not null)
            {
                if (!customerWallet.IsActive)
                {
                    await CreateCustomerWallet(command, customerWallet);
                }
                return new PaycoreResponse()
                {
                    IsSuccess = true
                };
            }

            await CreateCustomerWallet(command, customerWallets.FirstOrDefault());

            return new PaycoreResponse()
            {
                IsSuccess = true
            };
        }

        var bankingCustomerNo = GenerateBankingCustomerNo();

        var createCustomerRequest = new PaycoreCreateCustomerRequest
        {
            BankingCustomerNo = bankingCustomerNo,
            BranchCode = _paycoreSettings.VaultSettings.BranchCode.ToString(),
            CustomerGroupCode = command.CustomerGroupCode,
            Name = command.Name,
            ApplicationDate = DateTime.Now,
            CstCustomerAddresses = command.CustomerAddresses,
            CstCustomerCommunications = command.CustomerCommunications,
            Surname = command.Surname
        };
        var customerResponse = await _clientService.ExecuteAsync<string>(
            $"{_paycoreSettings.VaultSettings.BaseUrl}{_paycoreSettings.CreateCustomer}",
            PaycoreRequestType.Post,
            createCustomerRequest);

        if (!customerResponse.IsSuccess)
        {
            return new PaycoreResponse
            {
                IsSuccess = customerResponse.IsSuccess,
                Description = "Failed"//todo const
            };
        }

        var newCustomerWalletCard = new CustomerWalletCard
        {
            WalletNumber = command.WalletNumber,
            ProductCode = command.ProductCode,
            BankingCustomerNo = bankingCustomerNo,
            IsActive = true,
            CreatedBy = _contextProvider.CurrentContext.UserId,
        };

        await _customerWalletCard.AddAsync(newCustomerWalletCard);

        return new PaycoreResponse
        {
            IsSuccess = true,
            Description = "Success"//todo const
        };
    }

    private async Task CreateCustomerWallet(CreateCustomerCommand command, CustomerWalletCard customerWallet)
    {
        var customerWalletCard = new CustomerWalletCard
        {
            WalletNumber = command.WalletNumber,
            ProductCode = command.ProductCode,
            BankingCustomerNo = customerWallet.BankingCustomerNo,
            IsActive = true,
            CreatedBy = _contextProvider.CurrentContext.UserId
        };

        await _customerWalletCard.AddAsync(customerWalletCard);
    }

    public async Task<GetCustomerInformationResponse> GetCustomerInformationAsync(GetCustomerInformationQuery query)
    {
        var customerInformation = await _clientService.ExecuteAsync<GetCustomerInformationResponse>(
            $"{_paycoreSettings.VaultSettings.BaseUrl}{_paycoreSettings.GetCustomer}{query.BankingCustomerNo}",
            PaycoreRequestType.Get);

        if (!customerInformation.IsSuccess)
        {
            throw new InvalidOperationException();
        }
        return customerInformation.Result;
    }

    public async Task<List<GetCustomerCardsResponse>> GetCustomerCardsAsync(GetCustomerCardsQuery query)
    {
        var customerCards = await _clientService.ExecuteAsync<List<GetCustomerCardsResponse>>(
            $"{_paycoreSettings.VaultSettings.BaseUrl}{_paycoreSettings.GetCustomerCards}{query.BankingCustomerNo}",
            PaycoreRequestType.Get);

        if (!customerCards.IsSuccess)
        {
            throw new InvalidOperationException();
        }
        return customerCards.Result;
    }

    public async Task<List<GetCustomerLimitInfoResponse>> GetCustomerLimitInfoAsync(GetCustomerLimitInfoQuery query)
    {
        var customerLimit = await _clientService.ExecuteAsync<List<GetCustomerLimitInfoResponse>>(
            $"{_paycoreSettings.VaultSettings.BaseUrl}{_paycoreSettings.GetCustomerLimit}{query.BankingCustomerNo}",
            PaycoreRequestType.Get);

        if (!customerLimit.IsSuccess)
        {
            throw new InvalidOperationException();
        }
        return customerLimit.Result;
    }

    public async Task<PaycoreResponse> UpdateCustomerAsync(UpdateCustomerCommand command)
    {
        var updateCustomerRequest = new UpdateCustomerRequest
        {
            Name = command.Name,
            Surname = command.Surname,
            CustomerGroupCode = command.CustomerGroupCode,
            BankingCustomerNo = command.BankingCustomerNo
        };

        var updateCustomer = await _clientService.ExecuteAsync<string>(
            $"{_paycoreSettings.VaultSettings.BaseUrl}{_paycoreSettings.UpdateCustomer}",
            PaycoreRequestType.Put,
            updateCustomerRequest);

        if (!updateCustomer.IsSuccess)
        {
            return new PaycoreResponse
            {
                IsSuccess = updateCustomer.IsSuccess,
                Description = ResponseDescription.ERROR
            };
        }

        return new PaycoreResponse
        {
            IsSuccess = updateCustomer.IsSuccess
        };
    }

    public async Task<PaycoreResponse> UpdateCustomerCommunicationAsync(UpdateCustomerCommunicationCommand command)
    {
        var communicationRequest = new UpdateCommunicationRequest
        {
            BankingCustomerNo = command.BankingCustomerNo,
            CstCustomerCommunications = command.CustomerCommunications
        };

        var updateCommunication = await _clientService.ExecuteAsync<string>(
            $"{_paycoreSettings.VaultSettings.BaseUrl}{_paycoreSettings.UpdateCommunication}",
            PaycoreRequestType.Put,
            communicationRequest);

        if (!updateCommunication.IsSuccess)
        {
            return new PaycoreResponse
            {
                IsSuccess = updateCommunication.IsSuccess,
                Description = "Failed"//todo const
            };
        }

        return new PaycoreResponse
        {
            IsSuccess = updateCommunication.IsSuccess
        };
    }

    public async Task<PaycoreResponse> UpdateCustomerAddressAsync(UpdateCustomerAddressCommand command)
    {
        var addressRequest = new UpdateAddressRequest
        {
            CstCustomerAddresses = command.CustomerAddresses,
            BankingCustomerNo = command.BankingCustomerNo
        };

        var updateAddress = await _clientService.ExecuteAsync<string>(
            $"{_paycoreSettings.VaultSettings.BaseUrl}{_paycoreSettings.UpdateAddress}",
            PaycoreRequestType.Put,
            addressRequest);

        if (!updateAddress.IsSuccess)
        {
            return new PaycoreResponse
            {
                IsSuccess = updateAddress.IsSuccess,
                Description = "Failed"//todo const
            };
        }

        return new PaycoreResponse
        {
            IsSuccess = updateAddress.IsSuccess
        };
    }

    public async Task<PaycoreResponse> UpdateCustomerLimitAsync(UpdateCustomerLimitCommand command)
    {
        var limitRequest = new UpdateCustomerLimitRequest
        {
            BankingCustomerNo = command.BankingCustomerNo,
            LimitAssignType = command.LimitAssignType,
            CurrencyCode = command.CurrencyCode,
            IsLimitUsedControl = command.IsLimitUsedControl,
            MemoText = command.MemoText,
            NewLimit = command.NewLimit
        };

        var updateLimit = await _clientService.ExecuteAsync<string>(
            $"{_paycoreSettings.VaultSettings.BaseUrl}{_paycoreSettings.UpdateLimit}",
            PaycoreRequestType.Put,
            limitRequest);

        if (!updateLimit.IsSuccess)
        {
            return new PaycoreResponse
            {
                IsSuccess = updateLimit.IsSuccess,
                Description = "Failed"//todo const
            };
        }

        return new PaycoreResponse
        {
            IsSuccess = updateLimit.IsSuccess
        };
    }

    public string GenerateBankingCustomerNo()
    {
        var any = false;
        var bankingCustomerNo = string.Empty;
        do
        {
            bankingCustomerNo = _randomGenerator.GenerateSecureRandomNumber(16).ToString(CultureInfo.InvariantCulture);
            any = _customerWalletCard.GetAll().Any(s => s.BankingCustomerNo == bankingCustomerNo);
        }
        while (any);

        return bankingCustomerNo;
    }
}
