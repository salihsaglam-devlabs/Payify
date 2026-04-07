using AutoMapper;
using LinkPara.Card.Application.Commons.Interfaces;
using LinkPara.Card.Application.Commons.Models.PaycoreModels;
using LinkPara.Card.Application.Commons.Models.PaycoreModels.CardModels;
using LinkPara.Card.Application.Features.PaycoreServices.CardServices.Commands.CreateCard;
using LinkPara.Card.Application.Features.PaycoreServices.CardServices.Commands.UpdateCardAuthorizations;
using LinkPara.Card.Application.Features.PaycoreServices.CardServices.Commands.UpdateCardStatus;
using LinkPara.Card.Application.Features.PaycoreServices.CardServices.Queries.GetCardAuthorization;
using LinkPara.Card.Application.Features.PaycoreServices.CardServices.Queries.GetCardInformation;
using LinkPara.Card.Application.Features.PaycoreServices.CardServices.Queries.GetCardTransactions;
using LinkPara.Card.Infrastructure.Persistence;
using LinkPara.Card.Infrastructure.Services.PaycoreServices.Models;
using LinkPara.Card.Infrastructure.Services.PaycoreServices.Models.CardModels.Requests;
using LinkPara.Card.Infrastructure.Services.PaycoreServices.Models.CardModels.Responses;
using LinkPara.HttpProviders.Vault;
using Microsoft.Extensions.Configuration;

namespace LinkPara.Card.Infrastructure.Services.PaycoreServices.Services;

public class PaycoreCardService : IPaycoreCardService
{
    private readonly PaycoreClientService _clientService;
    private readonly IConfiguration _configuration;
    private readonly IVaultClient _vaultClient;
    private readonly PaycoreSettings _paycoreSettings;
    private readonly CardDbContext _dbContext;
    private readonly IMapper _mapper;

    public PaycoreCardService(PaycoreClientService clientService, 
        IConfiguration configuration, 
        IVaultClient vaultClient, 
        CardDbContext dbContext, 
        IMapper mapper)
    {
        _clientService = clientService;
        _configuration = configuration;
        _vaultClient = vaultClient;
        _configuration.GetSection(nameof(PaycoreSettings)).Bind(_paycoreSettings);
        _paycoreSettings.VaultSettings = _vaultClient.GetSecretValue<PaycoreVaultSettings>("CardSecrets", "PaycoreSettings");
        _dbContext = dbContext;
        _mapper = mapper;
    }
    public async Task<PaycoreResponse> CreateCardAsync(CreateCardCommand request)
    {      
        var createCardRequest = new PaycoreCreateCardRequest
        {
            BankingCustomerNo = request.BankingCustomerNo,
            BranchCode = _paycoreSettings.VaultSettings.BranchCode,
            ProductCode = _paycoreSettings.VaultSettings.ProductCode,
            CardLevel = _paycoreSettings.VaultSettings.CardLevel,
            CrdCardAccount = request.CardAccount,
            CrdCardDelivery = request.CardDelivery,
            EmbossMethod = "M",
            EmbossName1 = request.EmbossName1,
            EmbossName2 = request.EmbossName2
        };

        var cardResponse = await _clientService.ExecuteAsync<PaycoreCreateCardResponse>(
            $"{_paycoreSettings.VaultSettings.BaseUrl}{_paycoreSettings.CreateCard}",
            PaycoreRequestType.Post,
            createCardRequest);

        if (!cardResponse.IsSuccess)
        {
            return new PaycoreResponse
            {
                IsSuccess = cardResponse.IsSuccess,
                Description = "Error",//todo const
            };
        }

        return new PaycoreResponse
        {
            IsSuccess = cardResponse.IsSuccess,
            Description = "Success",//todo const
        };
    }
    public async Task<GetCardAuthorizationsResponse> GetCardAuthorizationsAsync(GetCardAuthorizationsQuery request)
    {
        var cardAuthorization = await _clientService.ExecuteAsync<PaycoreGetCardAuthorizationResponse>(
            $"{_paycoreSettings.VaultSettings.BaseUrl}{_paycoreSettings.GetCardAuthorization}{request.CardNumber}",
            PaycoreRequestType.Get);

        if (!cardAuthorization.IsSuccess)
        {
            return new GetCardAuthorizationsResponse
            {
                IsSuccess = cardAuthorization.IsSuccess,
                ErrorMessage = cardAuthorization.exception.message
            };
        }
        return new GetCardAuthorizationsResponse
        {
            IsSuccess = cardAuthorization.IsSuccess,
            CashTransactionPermission = cardAuthorization.Result.authDomCash,
            EcommercePermission = cardAuthorization.Result.authDomEcom,
            InternationalPermission = cardAuthorization.Result.authIntEcom,
            Non3DPermission = cardAuthorization.Result.authDomNoCVV2,
            ThreeDSecureType = cardAuthorization.Result.auth3dSecureType
        };
    }
    public async Task<GetCardInformationsResponse> GetCardInformationsAsync(GetCardInformationsQuery request)
    {
        var informationResponse = await _clientService.ExecuteAsync<PaycoreCardInformationResponse>(
            $"{_paycoreSettings.VaultSettings.BaseUrl}{_paycoreSettings.GetCardInformation}{request.CardNumber}",
            PaycoreRequestType.Get);

        if (!informationResponse.IsSuccess)
        {
            return new GetCardInformationsResponse
            {
                IsSuccess = informationResponse.IsSuccess,
                ErrorMessage = informationResponse.exception.message
            };
        }

        var cardInformations = informationResponse.Result.CardInformation.First();

        return new GetCardInformationsResponse
        {
            IsSuccess = informationResponse.IsSuccess,
            CardNumber = cardInformations.cardNo,
            Cvv2 = cardInformations.cvv2,
            ExpireDate = cardInformations.expiryDate
        };
    }
    public Task<GetCardTransactionsResponse> GetCardTransactionsAsync(GetCardTransactionsQuery request)
    {
        throw new NotImplementedException();
    }
    public async Task<PaycoreResponse> UpdateCardAuthorizationsAsync(UpdateCardAuthorizationCommand request)
    {

        var permissions = await _clientService.ExecuteAsync<PaycoreGetCardAuthorizationResponse>(
            $"{_paycoreSettings.VaultSettings.BaseUrl}{_paycoreSettings.GetCardAuthorization}{request.CardNumber}",
            PaycoreRequestType.Get);

        if (!permissions.IsSuccess)
        {
            return new PaycoreResponse
            {
                IsSuccess = permissions.IsSuccess,
                Description = permissions.exception.message
            };
        }

        var permissionsRequest = permissions.Result;

        permissionsRequest.CardNo = request.CardNumber;
        permissionsRequest.authDomEcom = request.EcommercePermission;
        permissionsRequest.authDomMoto = request.EcommercePermission;
        permissionsRequest.authDomNoCVV2 = request.Non3DPermission;
        permissionsRequest.authDomCash = request.CashTransactionPermission;
        permissionsRequest.auth3dSecureType = request.ThreeDSecureType;

        if (request.InternationalPermission)
        {
            permissionsRequest.authIntEcom = request.InternationalPermission;
            permissionsRequest.authIntMoto = request.InternationalPermission;
            permissionsRequest.authIntNoCVV2 = request.InternationalPermission;
            permissionsRequest.authIntCash = request.InternationalPermission;
        }

        var updatePermission = await _clientService.ExecuteAsync<PaycoreUpdateAuthorizationResponse>(
            $"{_paycoreSettings.VaultSettings.BaseUrl}{_paycoreSettings.GetCardAuthorization}",
            PaycoreRequestType.Put,
            permissionsRequest);

        if (!updatePermission.IsSuccess)
        {
            return new PaycoreResponse
            {
                IsSuccess = updatePermission.IsSuccess,
                Description = updatePermission.exception.message
            };
        }

        return new PaycoreResponse
        {
            IsSuccess = updatePermission.IsSuccess
        };
    }
    public async Task<UpdateCardStatusResponse> UpdateCardStatusAsync(UpdateCardStatusCommand request)
    {
        var updateCardRequest = new UpdateCardStatusRequest
        {
            CardNo = request.CardNumber,
            FreeText = request.Description,
            StatCode = request.StatusCode,
            StatusReasonCode = ""
        };

        var updateCard = await _clientService.ExecuteAsync<PaycoreUpdateCardStatusResponse>(
            $"{_paycoreSettings.VaultSettings.BaseUrl}{_paycoreSettings.UpdateCardStatus}",
            PaycoreRequestType.Put,
            updateCardRequest);

        if (!updateCard.IsSuccess)
        {
            return new UpdateCardStatusResponse
            {
                IsSuccess = updateCard.IsSuccess,
                ErrorMessage = updateCard.exception.message

            };
        }

        return new UpdateCardStatusResponse
        {
            IsSuccess = updateCard.IsSuccess
        };
    }
}
