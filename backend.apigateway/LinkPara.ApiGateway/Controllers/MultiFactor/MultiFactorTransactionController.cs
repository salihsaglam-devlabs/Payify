using LinkPara.ApiGateway.Commons.MultiFactorModels;
using LinkPara.ApiGateway.Services.MultiFactor;
using LinkPara.HttpProviders.MultiFactor.Models.PowerFactorModels.Request;
using LinkPara.HttpProviders.MultiFactor.Models.PowerFactorModels.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.Controllers.MultiFactor;

public class MultiFactorTransactionController : ApiControllerBase
{
    private readonly IMultiFactorTransactionHttpClient _multiFactorTransactionHttpClient;

    public MultiFactorTransactionController(IMultiFactorTransactionHttpClient multiFactorTransactionHttpClient)
    {
        _multiFactorTransactionHttpClient = multiFactorTransactionHttpClient;
    }
    
    [Authorize]
    [HttpPost]
    [Route("start-client-transaction")]
    public async Task<StartClientTransactionResponse> StartClientTransaction(StartClientTransactionRequest request)
    {
        return await _multiFactorTransactionHttpClient.StartClientTransaction(request);
    }
    
    [Authorize]
    [HttpPost]
    [Route("start-one-touch-transaction")]
    public async Task<OneTouchTransactionResponse> StartOneTouchTransaction(StartOneTouchTransactionRequest request)
    {
        return await _multiFactorTransactionHttpClient.StartOneTouchTransaction(request);
    }
    
    [AllowAnonymous]
    [HttpPost]
    [Route("check-transaction-approval")]
    public async Task<CheckTransactionApprovalResponse> CheckTransactionApproval(CheckTransactionApprovalRequest request)
    {
        return await _multiFactorTransactionHttpClient.CheckTransactionApproval(request);
    }
}