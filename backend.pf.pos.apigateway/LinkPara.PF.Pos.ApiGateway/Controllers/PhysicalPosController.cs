using LinkPara.PF.Pos.ApiGateway.Commons;
using LinkPara.PF.Pos.ApiGateway.Models.Requests;
using LinkPara.PF.Pos.ApiGateway.Models.Responses;
using LinkPara.PF.Pos.ApiGateway.Services.HttpClients;
using LinkPara.SharedModels.BusModels.Commands.Scheduler;
using LinkPara.SharedModels.BusModels.Commands.Scheduler.Enums;
using LinkPara.SharedModels.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace LinkPara.PF.Pos.ApiGateway.Controllers;

public class PhysicalPosController : ApiControllerBase
{
    private readonly IPaxHttpClient _paxHttpClient;
    private readonly IMerchantDeviceHttpClient _merchantDeviceHttpClient;
    private readonly IPaymentApiLog _paymentRequestResponseLog;
    
    public PhysicalPosController(
        IPaxHttpClient paxHttpClient, 
        IMerchantDeviceHttpClient merchantDeviceHttpClient, 
        IPaymentApiLog paymentRequestResponseLog)
    {
        _paxHttpClient = paxHttpClient;
        _merchantDeviceHttpClient = merchantDeviceHttpClient;
        _paymentRequestResponseLog = paymentRequestResponseLog;
    }
    
    /// <summary>
    /// Device Transaction Request
    /// </summary>
    /// <param name="request"></param>
    /// <param name="header"></param>
    /// <returns></returns>
    [Authorize(Policy = "RequireSignature")]
    [HttpPost("transaction")]
    public async Task<TransactionResponse> PaxTransactionAsync(TransactionRequest request,
        [FromQuery] RequestHeader header)
    {
        var apiKeys = await _merchantDeviceHttpClient.GetDeviceApiKeysAsync(header.PublicKey);
        
        var merchantRequest = new TransactionMerchantRequest(request)
        {
            ConversationId = header.ConversationId,
            PfMerchantId = apiKeys.MerchantId,
            ClientIpAddress = header.ClientIpAddress.FirstOrDefault(),
            SerialNumber = apiKeys.SerialNumber,
            Gateway = nameof(Gateway.PFPosGateway)
        };
    
        var response = await _paxHttpClient.PaxTransactionAsync(merchantRequest);
    
        await PaymentApiLogAsync(apiKeys.MerchantId, merchantRequest, response,
            response.ErrorCode, response.ErrorMessage, PaymentLogType.PhysicalPosTransaction);
    
        return response;
    }
    
    /// <summary>
    /// Device Parameter Request
    /// </summary>
    /// <param name="request"></param>
    /// <param name="header"></param>
    /// <returns></returns>
    [Authorize(Policy = "RequireSignature")]
    [HttpPost("parameter")]
    public async Task<ParameterResponse> PaxParameterAsync(ParameterRequest request,
        [FromQuery] RequestHeader header)
    {
        var apiKeys = await _merchantDeviceHttpClient.GetDeviceApiKeysAsync(header.PublicKey);
        
        var merchantRequest = new ParameterMerchantRequest(request)
        {
            ConversationId = header.ConversationId,
            PfMerchantId = apiKeys.MerchantId,
            ClientIpAddress = header.ClientIpAddress.FirstOrDefault(),
            SerialNumber = apiKeys.SerialNumber,
            Gateway = nameof(Gateway.PFPosGateway)
        };
    
        var response = await _paxHttpClient.PaxParameterAsync(merchantRequest);
    
        await PaymentApiLogAsync(apiKeys.MerchantId, merchantRequest, response,
            response.ErrorCode, response.ErrorMessage, PaymentLogType.PhysicalPosParameter);
    
        return response;
    }
    
    /// <summary>
    /// Device End of Day Request
    /// </summary>
    /// <param name="request"></param>
    /// <param name="header"></param>
    /// <returns></returns>
    [Authorize(Policy = "RequireSignature")]
    [HttpPost("eod")]
    public async Task<EndOfDayResponse> PaxEndOfDayAsync(EndOfDayRequest request,
        [FromQuery] RequestHeader header)
    {
        var apiKeys = await _merchantDeviceHttpClient.GetDeviceApiKeysAsync(header.PublicKey);
        
        var merchantRequest = new EndOfDayMerchantRequest(request)
        {
            ConversationId = header.ConversationId,
            PfMerchantId = apiKeys.MerchantId,
            ClientIpAddress = header.ClientIpAddress.FirstOrDefault(),
            SerialNumber = apiKeys.SerialNumber,
            Gateway = nameof(Gateway.PFPosGateway)
        };
    
        var response = await _paxHttpClient.PaxEndOfDayAsync(merchantRequest);
    
        await PaymentApiLogAsync(apiKeys.MerchantId, merchantRequest, response,
            response.ErrorCode, response.ErrorMessage, PaymentLogType.PhysicalPosEndOfDay);
    
        return response;
    }
    
    /// <summary>
    /// Device Reconciliation Request
    /// </summary>
    /// <param name="request"></param>
    /// <param name="header"></param>
    /// <returns></returns>
    [Authorize(Policy = "RequireSignature")]
    [HttpPost("reconciliation")]
    public async Task<ReconciliationResponse> PaxReconciliationAsync(ReconciliationRequest request,
        [FromQuery] RequestHeader header)
    {
        var apiKeys = await _merchantDeviceHttpClient.GetDeviceApiKeysAsync(header.PublicKey);
        
        var merchantRequest = new ReconciliationMerchantRequest(request)
        {
            ConversationId = header.ConversationId,
            PfMerchantId = apiKeys.MerchantId,
            ClientIpAddress = header.ClientIpAddress.FirstOrDefault(),
            SerialNumber = apiKeys.SerialNumber,
            Gateway = nameof(Gateway.PFPosGateway)
        };
    
        var response = await _paxHttpClient.PaxReconciliationAsync(merchantRequest);
    
        await PaymentApiLogAsync(apiKeys.MerchantId, merchantRequest, response,
            response.ErrorCode, response.ErrorMessage, PaymentLogType.PhysicalPosReconciliation);
    
        return response;
    }
    
    private async Task PaymentApiLogAsync(Guid merchantId, object request, object response, string errorCode, 
        string errorMessage, PaymentLogType logType)
    {
        await _paymentRequestResponseLog.SaveApiLogAsync(new PaymentApiLog
        {
            MerchantId = merchantId,
            Request = JsonConvert.SerializeObject(request),
            Response = JsonConvert.SerializeObject(response),
            ErrorCode = errorCode,
            ErrorMessage = errorMessage,
            PaymentType = logType
        });
    }
}