using LinkPara.Card.Application.Commons.Interfaces;
using LinkPara.Card.Application.Commons.Models.PaycoreModels;
using LinkPara.Card.Application.Commons.Models.PaycoreModels.SecurityModels;
using LinkPara.Card.Application.Features.PaycoreServices.CardPinServices.Commands.SetCardPin;
using LinkPara.Card.Application.Features.PaycoreServices.CardServices.Queries.GetClearCardNo;
using LinkPara.Card.Domain.Constants;
using LinkPara.Card.Infrastructure.Services.PaycoreServices.Models;
using LinkPara.Card.Infrastructure.Services.PaycoreServices.Models.CardModels.Requests;
using LinkPara.Card.Infrastructure.Services.PaycoreServices.Models.CardModels.Responses;
using LinkPara.HttpProviders.Vault;
using Microsoft.Extensions.Configuration;

namespace LinkPara.Card.Infrastructure.Services.PaycoreServices.Services;

public class PaycoreSecurityService : IPaycoreSecurityService
{
    private readonly PaycoreClientService _clientService;
    private readonly IConfiguration _configuration;
    private readonly IVaultClient _vaultClient;
    private readonly PaycoreSettings _paycoreSettings;
    private readonly IPinBlockService _pinBlockService;
    private readonly IPaycoreCardService _paycoreCardService;

    public PaycoreSecurityService(
        PaycoreClientService clientService,
        IConfiguration configuration,
        IVaultClient vaultClient,
        IPinBlockService pinBlockService,
        IPaycoreCardService paycoreCardService)
    {
        _clientService = clientService;
        _configuration = configuration;
        _vaultClient = vaultClient;

        _paycoreSettings = new PaycoreSettings();
        _configuration.GetSection(nameof(PaycoreSettings)).Bind(_paycoreSettings);
        _paycoreSettings.VaultSettings =
            _vaultClient.GetSecretValue<PaycoreVaultSettings>("CardSecrets", "PaycoreSettings");
        _pinBlockService = pinBlockService;
        _paycoreCardService = paycoreCardService;
    }

    public async Task<SetCardBinResponse> SetCardPinAsync(SetCardPinCommand command)
    {
        var clearCardResponse = await _paycoreCardService.GetClearCardNoAsync(
                   new GetClearCardNoQuery()
                   {
                       Cards = new string[] { command.TokenPan }
                   });
        
        var clearCardNo = clearCardResponse.First().CardNo;
        if (string.IsNullOrWhiteSpace(clearCardNo))
        {
            return new SetCardBinResponse
            {
                IsSuccess = false,
                Description = ResponseDescription.CLEAR_CARD_NO_EMPTY,
                Code = "CLEAR_CARD_NO_EMPTY",
                Data = string.Empty
            };
        }

        var pinBlock = await _pinBlockService.GenerateEncryptedPinBlock(
             command.ClearPin,
             clearCardNo);

        var request = new PaycoreSetCardPinRequest
        {
            CardNo = command.TokenPan,
            PinBlock = pinBlock.Data
        }; 
    
    var response = await _clientService.ExecuteAsync<PaycoreSetCardPinResponse>(
            $"{_paycoreSettings.VaultSettings.BaseUrl}{_paycoreSettings.SetCardPin}",
            PaycoreRequestType.Put,
            request);

        if (!response.IsSuccess)
        {
            return new SetCardBinResponse
            {
                IsSuccess = false,
                Description = response.exception.message,
                Code = "SET_PIN_ERROR",
                Data = string.Empty
};
        }

        return new SetCardBinResponse
        {
            IsSuccess = true,
            Description = "Success",
            Code = "00",
            Data = "SUCCESS"
        };   
    }
}