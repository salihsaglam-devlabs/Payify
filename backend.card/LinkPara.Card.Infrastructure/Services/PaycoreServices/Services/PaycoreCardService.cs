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

    public async Task<CreateCardResponse> CreateCardAsync(CreateCardCommand request)
    {
        var customerWalletCard = await _customerWalletCard
            .GetAll()
            .FirstOrDefaultAsync(x => x.WalletNumber == request.WalletNumber &&
                                      x.ProductCode == request.ProductCode &&
                                      x.IsActive == true);

        if (customerWalletCard is null)
        {
            return new CreateCardResponse()
            {
                IsSuccess = false,
                Description = "CustomerNotFound"
            };
        }

        if (!string.IsNullOrEmpty(customerWalletCard.CardNumber))
        {
            return new CreateCardResponse()
            {
                IsSuccess = false,
                Description = "CardExist"
            };
        }

        var createCardRequest = CreateCardRequest(request, customerWalletCard.BankingCustomerNo);

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

        var cardRequest = new PaycoreCardModel
        {
            BankingCustomerNo = bankingCustomerNo,
            BranchCode = _paycoreSettings.VaultSettings.BranchCode,
            ProductCode = request.ProductCode,
            CardLevel = _paycoreSettings.VaultSettings.CardLevel,
            CrdCardAccount = _crdCardAccount,
            EmbossMethod = EmbossMethods.MASS,
            EmbossName1 = request.EmbossName1,
            NoMoreRenewal = false,
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
        if (request.ProductCode != ProductCodes.SANAL)
        {
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
                cardRequest.CrdCardDelivery = cardDelivery;
                cardRequest.CourierCompanyCode = CourierCompanies.KURYE_NET;
            }
            else if (request.ProductCode == ProductCodes.TICARI)
            {
                cardDelivery.CardPostAddress.AddressType = AddressTypes.EV;
                cardDelivery.CardPostType = AddressTypes.OZEL;
                cardRequest.CrdCardDelivery = cardDelivery;
                cardRequest.CourierCompanyCode = CourierCompanies.KURYE_NET;
            }
            cardRequest.PersoCenterCode = PersoCenters.PLASTCARD;
        }
        return new PaycoreCreateCardRequest
        {
            CrdCard = cardRequest
        };
    }
    public async Task<GetCardAuthorizationsResponse> GetCardAuthorizationsAsync(GetCardAuthorizationsQuery request)
    {
        try
        {
            var cardAuthorization = await _clientService.ExecuteAsync<CrdCardAuth>(
                                  $"{_paycoreSettings.VaultSettings.BaseUrl}{_paycoreSettings.GetCardAuthorization}{request.CardNumber}",
                       PaycoreRequestType.Get);

            return new GetCardAuthorizationsResponse
            {
                IsSuccess = true,
                Description = ResponseDescription.SUCCESS,
                CrdCardAuth = cardAuthorization.Result
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
    public async Task<GetCardInformationsResponse> GetCardInformationsAsync(GetCardInformationsQuery request)
    {
        try
        {
            var informationResponse = await _clientService.ExecuteAsync<CrdCardInfo[]>(
                            $"{_paycoreSettings.VaultSettings.BaseUrl}{_paycoreSettings.GetCardInformation}{request.CardNumber}",
            PaycoreRequestType.Get);

            return new GetCardInformationsResponse
            {
                IsSuccess = true,
                Description = ResponseDescription.SUCCESS,
                CrdCardInfo = informationResponse.Result
            };
        }
        catch (Exception)
        {
            return new GetCardInformationsResponse
            {
                IsSuccess = false,
                Description = ResponseDescription.ERROR
            };
        }
    }
    public async Task<GetCardSensitiveDataResponse> GetCardSensitiveDataAsync(GetCardSensitiveDataQuery request)
    {
        try
        {
            var sensitiveResponse = await _clientService.ExecuteAsync<CrdCardSensitiveInfo[]>(
                              $"{_paycoreSettings.VaultSettings.BaseUrl}{_paycoreSettings.GetCardEncryptedCvv2AndExpireDate}{request.TokenPan}",
                PaycoreRequestType.Get);

            if (!sensitiveResponse.IsSuccess || !sensitiveResponse.Result.Any())
            {
                return new GetCardSensitiveDataResponse
                {
                    IsSuccess = false,
                    Description = sensitiveResponse.message
                };
            }

            if (string.IsNullOrWhiteSpace(sensitiveResponse.Result.FirstOrDefault().CardNo))
            {
                return new GetCardSensitiveDataResponse
                {
                    IsSuccess = false,
                    Description = ResponseDescription.CLEAR_CARD_NO_EMPTY
                };
            }

            if (string.IsNullOrWhiteSpace(sensitiveResponse.Result.FirstOrDefault().ExpiryDate))
            {
                return new GetCardSensitiveDataResponse
                {
                    IsSuccess = false,
                    Description = ResponseDescription.EXPIREDATEANDCVV2_ERROR
                };
            }

            var clearExpiryDateAndCvv2 = await _pinBlockService.DecryptEncryptedPinBlock(
                sensitiveResponse.Result.FirstOrDefault().ExpiryDate,
                sensitiveResponse.Result.FirstOrDefault().CardNo);

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

            CrdCardSensitiveInfo info = new CrdCardSensitiveInfo
            {
                CardNo = sensitiveResponse.Result.FirstOrDefault().CardNo,
                BankingCustomerNo = sensitiveResponse.Result.FirstOrDefault().BankingCustomerNo,
                CustomerNo = sensitiveResponse.Result.FirstOrDefault().CustomerNo,
                ExpiryDate = expiryDate,
                Cvv2 = cvv2
            };

            return new GetCardSensitiveDataResponse
            {
                IsSuccess = true,
                Description = ResponseDescription.SUCCESS,
                CrdCardInfo = new CrdCardSensitiveInfo[1] {info}
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
        try
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
        catch (Exception ex) 
        {
            return new PaycoreResponse
            {
                IsSuccess = false,
                Description = ex.Message
            };
        }
    }
    public async Task<PaycoreResponse> UpdateCardStatusAsync(UpdateCardStatusCommand request)
    {
        try
        {
            var updateCardRequest = new UpdateCardStatusRequest
            {
                CardNo = request.CardNumber,
                FreeText = request.Description,
                StatCode = request.StatusCode,
                StatusReasonCode = request.StatusReasonCode
            };

            var updateCard = await _clientService.ExecuteAsync<PaycoreBaseResponse>(
                $"{_paycoreSettings.VaultSettings.BaseUrl}{_paycoreSettings.UpdateCardStatus}",
                PaycoreRequestType.Put,
                updateCardRequest);

            if (!updateCard.IsSuccess)
            {
                return new PaycoreResponse
                {
                    IsSuccess = updateCard.IsSuccess,
                    Description = updateCard.message
                };
            }

            var customerWalletCard = await _customerWalletCard
                        .GetAll()
                        .FirstOrDefaultAsync(x => x.CardNumber == request.CardNumber);

            customerWalletCard.IsActive = request.StatusCode == CardStatus.Cancelled ? false : true;
            await _customerWalletCard.UpdateAsync(customerWalletCard);

            return new PaycoreResponse
            {
                IsSuccess = updateCard.IsSuccess
            };
        }
        catch (Exception ex)
        {
            return new PaycoreResponse
            {
                IsSuccess = false,
                Description = ex.Message    
            };
        }
        
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
            CardLastCourierActivities = activities,
            Description = ResponseDescription.SUCCESS
        };
    }
    public async Task<PaycoreResponse> AddAdditionalLimitRestrictionAsync(AddAdditionalLimitRestrictionCommand command)
    {
        try
        {
            var addAdditionalLimitRestrictionRequest = new PaycoreAddAdditionalLimitRestrictionRequest
            {
                CardNo = command.CardNo,
                AdditionalLimits = command.AdditionalLimits,
                LimitRestrictions = command.LimitRestrictions
            };

            var addAdditionalLimitRestriction = await _clientService.ExecuteAsync<PaycoreBaseResponse>(
                $"{_paycoreSettings.VaultSettings.BaseUrl}{_paycoreSettings.AddAdditionalLimitRestriction}",
                PaycoreRequestType.Put,
                addAdditionalLimitRestrictionRequest);

        if (!addAdditionalLimitRestriction.IsSuccess)
        {
            return new AddAdditionalLimitRestrictionResponse
            {
                IsSuccess = addAdditionalLimitRestriction.IsSuccess,
                Description = "Service Error"
            };
        }

            return new AddAdditionalLimitRestrictionResponse
            {
                IsSuccess = addAdditionalLimitRestriction.IsSuccess,
                Description = ResponseDescription.SUCCESS
            };
        }
        catch (Exception ex)
        {
            return new AddAdditionalLimitRestrictionResponse
            {
                IsSuccess = false,
                Description = ex.Message    
            };
        }
        
    }
    public async Task<GetClearCardNoResponse> GetClearCardNoAsync(GetClearCardNoQuery request)
    {
        try
        {
            var clearCardResponse = await _clientService.ExecuteAsync<CrdClearCardInfo[]>(
                   $"{_paycoreSettings.VaultSettings.BaseUrl}{_paycoreSettings.GetClearCardNo}[{request.Cards[0]}]",
                    PaycoreRequestType.Get);

            return new GetClearCardNoResponse
            {
                IsSuccess = true,
                Description = ResponseDescription.SUCCESS,
                CrdClearCardInfo = clearCardResponse.Result
            };
        }
        catch (Exception)
        {
            return new GetClearCardNoResponse
            {
                IsSuccess = false,
                Description = ResponseDescription.ERROR
            };
        }
    }
    public async Task<CardRenewalResponse> CardRenewalAsync(CardRenewalCommand command)
    {
        try
        {
            var cardRenewalResponse = await _clientService.ExecuteAsync<PaycoreCardRenewalResponse>(
                      $"{_paycoreSettings.VaultSettings.BaseUrl}{_paycoreSettings.CardRenewal}",
                       PaycoreRequestType.Post,
                       command);

            if (!cardRenewalResponse.IsSuccess)
            {
                return new CardRenewalResponse
                {
                    IsSuccess = cardRenewalResponse.IsSuccess,
                    Description = cardRenewalResponse.message
                };
            }

            return new CardRenewalResponse
            {
                IsSuccess = true,
                Description = ResponseDescription.SUCCESS,
                CardNo = cardRenewalResponse.Result.cardNo,
                ExpiryDate = cardRenewalResponse.Result.expiryDate,
                PanSeqNumber = cardRenewalResponse.Result.panSeqNumber
            };
        }
        catch (Exception ex) 
        {
            return new CardRenewalResponse
            {
                IsSuccess = false,
                Description = ex.Message
            };
        }
    }
    public async Task<GetCardStatusResponse> GetCardStatusAsync(GetCardStatusQuery request)
    {
        try
        {
            List<string> cardList = new List<string>();
            foreach (var card in request.CardNos)
            {
                cardList.Add(card);
            }

            var cardStatusResponse = await _clientService.ExecuteAsync<CrdCardStatus[]>(
                $"{_paycoreSettings.VaultSettings.BaseUrl}{_paycoreSettings.GetCardStatus}[{cardList}]",
                 PaycoreRequestType.Get);


            return new GetCardStatusResponse
            {
                IsSuccess = cardStatusResponse.IsSuccess,
                Description = ResponseDescription.SUCCESS,
                Result = cardStatusResponse.Result
            };
        }
        catch (Exception ex)
        {
            return new GetCardStatusResponse
            {
                IsSuccess = false,
                Description = ex.Message
            };
        }
    }
}
