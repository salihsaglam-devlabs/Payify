using LinkPara.Card.Application.Commons.Interfaces;
using LinkPara.Card.Application.Commons.Models.PaycoreModels;
using LinkPara.Card.Application.Commons.Models.PaycoreModels.CardModels;
using LinkPara.Card.Application.Features.PaycoreServices.CardServices.Commands.CreateCard;
using LinkPara.Card.Application.Features.PaycoreServices.CardServices.Commands.UpdateCardAuthorizations;
using LinkPara.Card.Application.Features.PaycoreServices.CardServices.Commands.UpdateCardStatus;
using LinkPara.Card.Application.Features.PaycoreServices.CardServices.Queries.GetCardAuthorization;
using LinkPara.Card.Application.Features.PaycoreServices.CardServices.Queries.GetCardInformation;
using LinkPara.Card.Application.Features.PaycoreServices.CardServices.Queries.GetCardSensitiveData;
using LinkPara.Card.Application.Features.PaycoreServices.CardServices.Queries.GetCardTransactions;
using LinkPara.Card.Application.Features.PaycoreServices.CardServices.Queries.GetClearCardNo;
using LinkPara.Card.Domain.Constants;
using LinkPara.Card.Domain.Entities;
using LinkPara.Card.Infrastructure.Services.PaycoreServices.Models;
using LinkPara.Card.Infrastructure.Services.PaycoreServices.Models.CardModels.Requests;
using LinkPara.Card.Infrastructure.Services.PaycoreServices.Models.CardModels.Responses;
using LinkPara.Card.Infrastructure.Services.PaycoreServices.Models.Response;
using LinkPara.HttpProviders.Vault;
using LinkPara.SharedModels.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace LinkPara.Card.Infrastructure.Services.PaycoreServices.Services;

public class PaycoreCardService : IPaycoreCardService
{
    private readonly PaycoreClientService _clientService;
    private readonly IConfiguration _configuration;
    private readonly IVaultClient _vaultClient;
    private readonly PaycoreSettings _paycoreSettings;
    private readonly IPinBlockService _pinBlockService;
    private readonly IGenericRepository<CustomerWalletCard> _customerWalletCard;

    public PaycoreCardService(PaycoreClientService clientService,
        IConfiguration configuration,
        IVaultClient vaultClient,
        IPinBlockService pinBlockService,
        IGenericRepository<CustomerWalletCard> customerWalletCard)
    {
        _clientService = clientService;
        _configuration = configuration;
        _vaultClient = vaultClient;
        _pinBlockService = pinBlockService;

        _paycoreSettings = new PaycoreSettings();
        _configuration.GetSection(nameof(PaycoreSettings)).Bind(_paycoreSettings);
        _paycoreSettings.VaultSettings =
        _vaultClient.GetSecretValue<PaycoreVaultSettings>("CardSecrets", "PaycoreSettings");
        _customerWalletCard = customerWalletCard;
    }

    public async Task<PaycoreResponse> CreateCardAsync(CreateCardCommand request)
    {
        var customerWalletCard = await _customerWalletCard
            .GetAll()
            .FirstOrDefaultAsync(x => x.WalletNumber == request.WalletNumber &&
                                      x.ProductCode == request.ProductCode &&
                                      x.IsActive == true);

        if (customerWalletCard is null)
        {
            return new PaycoreResponse()
            {
                IsSuccess = false,
                Description = "CustomerNotFound"
            };
        }

        if (!string.IsNullOrEmpty(customerWalletCard.CardNumber))
        {
            return new PaycoreResponse()
            {
                IsSuccess = false,
                Description = "CardExist"
            };
        }

        var createCardRequest = CreateCardRequest(request, customerWalletCard.BankingCustomerNo);

        var cardDelivery = new CrdCardDelivery();
        cardDelivery.CardPostAddress = new PaycoreAddress()
        {
            AddressType = request.CardDelivery.CardPostAddress.AddressType,
            CityCode = request.CardDelivery.CardPostAddress.CityCode,
            CountryCode = request.CardDelivery.CardPostAddress.CountryCode,
            TownCode = request.CardDelivery.CardPostAddress.TownCode
        };

        if (request.ProductCode == ProductCodes.FIZIKI)
        {
            cardDelivery.CardPostAddress.AddressType = AddressTypes.GECERLI;
            createCardRequest.CrdCardDelivery = cardDelivery;
            createCardRequest.CourierCompanyCode = CourierCompanies.KURYE_NET;
        }
        else if (request.ProductCode == ProductCodes.TICARI)
        {
            cardDelivery.CardPostAddress.AddressType = AddressTypes.EV;
            cardDelivery.CardPostType = AddressTypes.OZEL;
            createCardRequest.CrdCardDelivery = cardDelivery;
            createCardRequest.CourierCompanyCode = CourierCompanies.KURYE_NET;
        }

        var cardResponse = await _clientService.ExecuteAsync<PaycoreCreateCardResponse>(
           $"{_paycoreSettings.VaultSettings.BaseUrl}{_paycoreSettings.CreateCard}",
            PaycoreRequestType.Post,
            createCardRequest);

        if (!cardResponse.IsSuccess)
        {
            return new CreateCardResponse
            {
                IsSuccess = cardResponse.IsSuccess,
                Description = ResponseDescription.ERROR,
            };
        }

        customerWalletCard.CardNumber = cardResponse.Result.cardNo;
        customerWalletCard.CardName = request.CardName;
        customerWalletCard.CardStatus = CardStatus.Open;
        await _customerWalletCard.UpdateAsync(customerWalletCard);

        return new CreateCardResponse
        {
            IsSuccess = cardResponse.IsSuccess,
            CardNo = cardResponse.Result.cardNo,
            Description = ResponseDescription.SUCCESS
        };
    }
    private PaycoreCreateCardRequest CreateCardRequest(CreateCardCommand request, string bankingCustomerNo)
    {
        var cardAccount = new CrdAccount();
        cardAccount.CrdAccountCommunication = new CrdAccountCommunication();
        cardAccount.CrdAccountCommunication.Email = request.CardAccount.CrdAccountCommunication.Email;
        cardAccount.CrdAccountCommunication.MobilePhone = request.CardAccount.CrdAccountCommunication.MobilePhone;

        var _crdCardAccount = new CrdCardAccount
        {
            CrdAccount = cardAccount
        };

        return new PaycoreCreateCardRequest
        {
            BankingCustomerNo = bankingCustomerNo,
            BranchCode = _paycoreSettings.VaultSettings.BranchCode,
            ProductCode = request.ProductCode,
            CardLevel = _paycoreSettings.VaultSettings.CardLevel,
            CrdCardAccount = _crdCardAccount,
            EmbossMethod = EmbossMethods.MASS,
            EmbossName1 = request.EmbossName1,
            NoMoreRenewal = false,
            PersoCenterCode = PersoCenters.PLASTCARD,
            CrdCardAuth = new CrdCardAuth
            {
                AuthDomEcom = true,
                AuthDomMoto = true,
                AuthDomNoCVV2 = true,
                AuthDomCash = true,
                AuthDomContactless = true,
                AuthIntEcom = true,
                AuthIntMoto = true,
                AuthIntNoCVV2 = true,
                AuthIntContactless = true,
                AuthIntCash = true,
                AuthIntPosSale = true,
                Auth3dSecure = true,
                Auth3dSecureType = ThreeDSecureTypes.SMS,
                AuthCloseInstallment = false,
                AuthDomCasino = false,
                AuthIntCasino = false,
                VisaAccountUpdaterOpt = false
            }
        };
    }
    public async Task<GetCardAuthorizationsResponse> GetCardAuthorizationsAsync(GetCardAuthorizationsQuery request)
    {
        try
        {
            var cardAuthorization = await _clientService.ExecuteGenericAsync<CrdCardAuth>(
                                  $"{_paycoreSettings.VaultSettings.BaseUrl}{_paycoreSettings.GetCardAuthorization}{request.CardNumber}",
                       PaycoreRequestType.Get);

            return new GetCardAuthorizationsResponse
            {
                IsSuccess = true,
                Description = ResponseDescription.SUCCESS,
                CrdCardAuth = cardAuthorization
            };
        }
        catch (Exception)
        {
            return new GetCardAuthorizationsResponse
            {
                IsSuccess = false,
                Description = ResponseDescription.ERROR
            };
        }
    }
    public async Task<List<GetCardInformationsResponse>> GetCardInformationsAsync(GetCardInformationsQuery request)
    {
        var informationResponse = await _clientService.ExecuteGenericAsync<List<PaycoreGetCardInformationResponse>>(
                            $"{_paycoreSettings.VaultSettings.BaseUrl}{_paycoreSettings.GetCardInformation}{request.TokenPan}",
            PaycoreRequestType.Get);

        if (informationResponse==null)
        {
            throw new InvalidOperationException();
        }
        var result = new List<GetCardInformationsResponse>();
        foreach (var cardInformations in informationResponse)
        {
           result.Add(new GetCardInformationsResponse
            {
                IsSuccess = true,
                ApplicationRefNo = cardInformations.ApplicationRefNo,
                BankingCustomerNo = cardInformations.BankingCustomerNo,
                BarcodeNo = cardInformations.BarcodeNo,
                BatchBarcodeNo = cardInformations.BatchBarcodeNo,
                CardNo = cardInformations.CardNo,
                CustomerNo = cardInformations.CustomerNo,
                CustomerType = cardInformations.CustomerType,
                Dci = cardInformations.Dci,
                ProductCode = cardInformations.ProductCode,
                Segment = cardInformations.Segment
            });     
        }

        return result;
    }
    public async Task<GetCardSensitiveDataResponse> GetCardSensitiveDataAsync(GetCardSensitiveDataQuery request)
    {
        try
        {
            var sensitiveResponse = await _clientService.ExecuteGenericAsync<PaycoreGetCardEncryptedCvv2AndExpireDateResponse>(
                              $"{_paycoreSettings.VaultSettings.BaseUrl}{_paycoreSettings.GetCardEncryptedCvv2AndExpireDate}{request.TokenPan}",
                PaycoreRequestType.Get);

            if (sensitiveResponse == null)
            {
                throw new InvalidOperationException();
            }

            if (string.IsNullOrWhiteSpace(sensitiveResponse.cardNo))
            {
                return new GetCardSensitiveDataResponse
                {
                    IsSuccess = false,
                    Description = ResponseDescription.CLEAR_CARD_NO_EMPTY
                };
            }

            if (string.IsNullOrWhiteSpace(sensitiveResponse.expiryDateAndCvv2))
            {
                return new GetCardSensitiveDataResponse
                {
                    IsSuccess = false,
                    Description = ResponseDescription.EXPIREDATEANDCVV2_ERROR
                };
            }

            var clearExpiryDateAndCvv2 = await _pinBlockService.DecryptEncryptedPinBlock(
                sensitiveResponse.expiryDateAndCvv2,
                sensitiveResponse.cardNo);

            if (string.IsNullOrWhiteSpace(clearExpiryDateAndCvv2.Data) || clearExpiryDateAndCvv2.Data.Length < 7)
            {
                return new GetCardSensitiveDataResponse
                {
                    IsSuccess = false,
                    Description = ResponseDescription.EXPIREDATEANDCVV2_FORMAT_ERROR
                };
            }

            var expiryDate = clearExpiryDateAndCvv2.Data.Substring(0, 4); 
            var cvv2 = clearExpiryDateAndCvv2.Data.Substring(4);         

            return new GetCardSensitiveDataResponse
            {
                IsSuccess = true,
                Description = ResponseDescription.SUCCESS,
                CardNo = sensitiveResponse.cardNo,
                ExpiryDate = expiryDate,
                Cvv2 = cvv2,
                CustomerNo = sensitiveResponse.customerNo,
                BankingCustomerNo = sensitiveResponse.bankingCustomerNo
            };
        }
        catch (Exception ex)
        {
            return new GetCardSensitiveDataResponse
            {
                IsSuccess = false,
                Description = ex.Message
            };
        }
    }
    public Task<GetCardTransactionsResponse> GetCardTransactionsAsync(GetCardTransactionsQuery request)
    {
        throw new NotImplementedException();
    }
    public async Task<PaycoreResponse> UpdateCardAuthorizationsAsync(UpdateCardAuthorizationCommand request)
    {
        PaycoreUpdateCardAuthorizationsRequest updateCardAuthorizationRequest = new PaycoreUpdateCardAuthorizationsRequest
        {
            CardNo = request.CardNumber,
            AuthDomEcom = request.CrdCardAuth.AuthDomEcom,
            AuthDomMoto = request.CrdCardAuth.AuthDomMoto,
            AuthDomNoCVV2 = request.CrdCardAuth.AuthDomNoCVV2,
            AuthDomCash = request.CrdCardAuth.AuthDomCash,
            AuthDomContactless = request.CrdCardAuth.AuthDomContactless,
            AuthIntEcom = request.CrdCardAuth.AuthIntEcom,
            AuthIntMoto = request.CrdCardAuth.AuthIntMoto,
            AuthIntNoCVV2 = request.CrdCardAuth.AuthIntNoCVV2,
            AuthIntContactless = request.CrdCardAuth.AuthIntContactless,
            AuthIntCash = request.CrdCardAuth.AuthIntCash,
            AuthIntPosSale = request.CrdCardAuth.AuthIntPosSale,
            Auth3dSecure = request.CrdCardAuth.Auth3dSecure,
            Auth3dSecureType = request.CrdCardAuth.Auth3dSecureType,
            AuthCloseInstallment = request.CrdCardAuth.AuthCloseInstallment,
            AuthDomCasino = request.CrdCardAuth.AuthDomCasino,
            AuthIntCasino = request.CrdCardAuth.AuthIntCasino,
            VisaAccountUpdaterOpt = request.CrdCardAuth.VisaAccountUpdaterOpt
        };

        var updateCardAuthorizationsResponse = await _clientService.ExecuteAsync<PaycoreBaseResponse>(
            $"{_paycoreSettings.VaultSettings.BaseUrl}{_paycoreSettings.UpdateCardAuthorization}",
            PaycoreRequestType.Put,
            updateCardAuthorizationRequest);

        if (!updateCardAuthorizationsResponse.IsSuccess)
        {
            return new PaycoreResponse
            {
                IsSuccess = false,
                Description = updateCardAuthorizationsResponse.message
            };
        }

        return new PaycoreResponse
        {
            IsSuccess = true,
            Description = updateCardAuthorizationsResponse.message
        };
    }
    public async Task<PaycoreResponse> UpdateCardStatusAsync(UpdateCardStatusCommand request)
    {
        var updateCardRequest = new UpdateCardStatusRequest
        {
            CardNo = request.CardNumber,
            FreeText = request.Description,
            StatCode = request.StatusCode,
            StatusReasonCode = request.StatusReasonCode
        };

        var updateCard = await _clientService.ExecuteAsync<PaycoreUpdateCardStatusResponse>(
            $"{_paycoreSettings.VaultSettings.BaseUrl}{_paycoreSettings.UpdateCardStatus}",
            PaycoreRequestType.Put,
            updateCardRequest);

        if (!updateCard.IsSuccess)
        {
            return new PaycoreResponse
            {
                IsSuccess = updateCard.IsSuccess,
                Description = updateCard.exception.message
            };
        }

        return new PaycoreResponse
        {
            IsSuccess = updateCard.IsSuccess
        };
    }
    public async Task<GetCardLastCourierActivityResponse> GetCardLastCorierActivityAsync(GetCardLastCourierActivityQuery request)
    {
        var courierResponse = await _clientService.ExecuteGenericAsync<PaycoreCardLastCourierActivity[]>(
            $"{_paycoreSettings.VaultSettings.BaseUrl}{_paycoreSettings.GetCardLastCourierActivity}{request.CardNumber}",
            PaycoreRequestType.Get);

        if (courierResponse == null || !courierResponse.Any())
        {
            return new GetCardLastCourierActivityResponse
            {
                IsSuccess = false,
                Description = ResponseDescription.ERROR

            };
        }

        List<CardLastCourierActivity> activities = new List<CardLastCourierActivity>();

        foreach (var activity in courierResponse)
        {
            activities.Add(new CardLastCourierActivity
            {
                BranchCode = activity.branchCode,
                Brand = activity.brand,
                CardDeliveryRecipientName = activity.cardDeliveryRecipientName,
                CardNo = activity.cardNo,
                CompanyName = activity.companyName,
                CourierActivityCode = activity.courierActivityCode,
                CourierAddress = activity.courierAddress,
                CourierCity = activity.courierCity,
                CourierCompanyCode = activity.courierCompanyCode,
                CourierCompanyName = activity.courierCompanyName,
                CourierStatChangeDate = activity.courierStatChangeDate,
                CourierStatChangeTime = activity.courierStatChangeTime,
                CourierStatusDescription = activity.courierStatusDescription,
                EmbossDate = activity.embossDate,
                EmbossName1 = activity.embossName1
            });
        }


        return new GetCardLastCourierActivityResponse
        {
           IsSuccess = true,
           CardLastCourierActivities =  activities,
           Description = ResponseDescription.SUCCESS
        };
    }
    public async Task<AddAdditionalLimitRestrictionResponse> AddAdditionalLimitRestrictionAsync(AddAdditionalLimitRestrictionCommand command)
    {
        var addAdditionalLimitRestrictionRequest = new PaycoreAddAdditionalLimitRestrictionRequest
        {
            CardNo = command.CardNo,
            AdditionalLimits = command.AdditionalLimits,
            LimitRestrictions = command.LimitRestrictions
        };

        var addAdditionalLimitRestriction = await _clientService.ExecuteAsync<PaycoreAddAdditionalLimitRestrictionResponse>(
            $"{_paycoreSettings.VaultSettings.BaseUrl}{_paycoreSettings.AddAdditionalLimitRestriction}",
            PaycoreRequestType.Put,
            addAdditionalLimitRestrictionRequest);

        if (!addAdditionalLimitRestriction.IsSuccess)
        {
            return new AddAdditionalLimitRestrictionResponse
            {
                IsSuccess = addAdditionalLimitRestriction.IsSuccess,
                Description = addAdditionalLimitRestriction.exception.message

            };
        }

        return new AddAdditionalLimitRestrictionResponse
        {
            IsSuccess = addAdditionalLimitRestriction.IsSuccess
        };
    }
    public async Task<List<GetClearCardNoResponse>> GetClearCardNoAsync(GetClearCardNoQuery request)
    {
        var clearCardResponse = await _clientService.ExecuteGenericAsync<List<PaycoreGetClearCardNoResponse>>(
           $"{_paycoreSettings.VaultSettings.BaseUrl}{_paycoreSettings.GetClearCardNo}[{request.Cards[0]}]",
            PaycoreRequestType.Get);

        if (clearCardResponse == null)
        {
            throw new InvalidOperationException();
        }
        var result = new List<GetClearCardNoResponse>();
        foreach (var card in clearCardResponse)
        {
            result.Add(new GetClearCardNoResponse
            {
                IsSuccess = true,
                CardNo = card.cardNo,
                CardToken = card.cardToken,
                CardUniqId = card.cardUniqId
            });
        }

        return result;
    }

}
