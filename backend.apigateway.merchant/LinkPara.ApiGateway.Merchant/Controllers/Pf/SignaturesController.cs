using LinkPara.ApiGateway.Merchant.Commons.Helpers;
using LinkPara.ApiGateway.Merchant.Services.Pf.HttpClients;
using LinkPara.ApiGateway.Merchant.Services.Pf.InternalServices;
using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Enums;
using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.Merchant.Controllers.Pf;

public class SignaturesController : ApiControllerBase
{
    private readonly IMerchantHttpClient _merchantHttpClient;
    private readonly ISignatureGenerator _signatureGenerator;
    private readonly IValidateUserService _validateUserService;

    public SignaturesController(IMerchantHttpClient merchantHttpClient,
        ISignatureGenerator signatureGenerator,
        IValidateUserService validateUserService)
    {
        _merchantHttpClient = merchantHttpClient;
        _signatureGenerator = signatureGenerator;
        _validateUserService = validateUserService;
    }

    /// <summary>
    /// Generate Hmac authentication signature
    /// and other required fields for authentication (test) (dev only)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    /// <exception cref="InvalidParameterException"></exception>
    [HttpPost("generate-signature")]
    [Authorize(Policy = "Signature:Create")]
    public async Task<SignatureDataResponse> GenerateSignature(SignatureDataRequest request)
    {
        var apiKeys = await _merchantHttpClient.GetApiKeysAsync(request.PublicKey);

        if (apiKeys.MerchantNumber != request.MerchantNumber)
            throw new InvalidParameterException(request.MerchantNumber);
        
        await _validateUserService.ValidateUserAsync(request.PublicKey, UserId);

        return await _signatureGenerator.GenerateSignatureAsync(request);
    }
}
