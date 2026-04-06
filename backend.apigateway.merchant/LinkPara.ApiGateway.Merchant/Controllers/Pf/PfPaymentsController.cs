using LinkPara.ApiGateway.Merchant.Services.Pf.HttpClients;
using LinkPara.ApiGateway.Merchant.Services.Pf.InternalServices;
using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.Merchant.Controllers.Pf;

public class PfPaymentsController : ApiControllerBase
{
    private readonly IPfPaymentHttpClient _paymentHttpClient;
    private readonly IValidateUserService _validateUserService;

    public PfPaymentsController(IPfPaymentHttpClient paymentHttpClient,
        IValidateUserService validateUserService)
    {
        _paymentHttpClient = paymentHttpClient;
        _validateUserService = validateUserService;
    }

    /// <summary>
    /// Payment page Provision
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("")]
    [Authorize(Policy = "PfPayment:Create")]
    public async Task<PfProvisionResponse> SavePaymentAsync(ProvisionRequest request)
    {
        await _validateUserService.ValidateUserAsync(request.PublicKey, UserId);

        return await _paymentHttpClient.SavePaymentAsync(request);
    }

    /// <summary>
    /// Get 3D Session
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("getthreedsession")]
    [Authorize(Policy = "PfPayment:Create")]
    public async Task<GetThreeDSessionResponse> GetThreeDSessionAsync(GetThreeDSessionRequest request)
    {
        await _validateUserService.ValidateUserAsync(request.PublicKey, UserId);

        return await _paymentHttpClient.GetThreeDSessionAsync(request);
    }

    /// <summary>
    /// Get 3D Session Result
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("getthreedsessionresult")]
    [Authorize(Policy = "PfPayment:Create")]
    public async Task<GetThreeDSessionResultResponse> GetThreeDSessionResultAsync(GetThreeDSessionResultRequest request)
    {
        await _validateUserService.ValidateUserAsync(request.PublicKey, UserId);

        return await _paymentHttpClient.GetThreeDSessionResultAsync(request);
    }

    /// <summary>
    /// Return
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("return")]
    [Authorize(Policy = "PfReturnTransaction:Create")]
    public async Task<PfReturnResponse> PfReturnTransaction(ReturnRequest request)
    {
        await _validateUserService.ValidateUserAsync(request.PublicKey, UserId);

        return await _paymentHttpClient.ReturnPaymentAsync(request);
    }

    /// <summary>
    /// Reverse
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("reverse")]
    [Authorize(Policy = "PfPayment:Create")]
    public async Task<PfReverseResponse> ReversePaymentAsync(ReverseRequest request)
    {
        await _validateUserService.ValidateUserAsync(request.PublicKey, UserId);

        return await _paymentHttpClient.ReversePaymentAsync(request);
    }
    
    /// <summary>
    /// Returns a payment detail
    /// </summary>
    /// <returns></returns>
    [Authorize(Policy = "PfPayment:Create")]
    [HttpPost("inquire")]
    public async Task<InquireResponse> InquireAsync(InquireRequest request)
    {
        await _validateUserService.ValidateUserAsync(request.PublicKey, UserId);

        return await _paymentHttpClient.InquirePaymentAsync(request);
    }
}
