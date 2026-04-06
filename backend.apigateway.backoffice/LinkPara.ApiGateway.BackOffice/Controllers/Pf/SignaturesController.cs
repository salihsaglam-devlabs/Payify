using LinkPara.ApiGateway.BackOffice.Commons.Helpers;
using LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Controllers.Pf;

public class SignaturesController : ApiControllerBase
{
    private readonly IMerchantHttpClient _merchantHttpClient;
    private readonly ISignatureGenerator _signatureGenerator;

    public SignaturesController(IMerchantHttpClient merchantHttpClient,
        ISignatureGenerator signatureGenerator)
    {
        _merchantHttpClient = merchantHttpClient;
        _signatureGenerator = signatureGenerator;
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

        return await _signatureGenerator.GenerateSignatureAsync(request);
    }
}
