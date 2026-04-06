using LinkPara.ApiGateway.BackOffice.Services.EMoney.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Controllers.Emoney;

public class ManualTransfersController : ApiControllerBase
{
    private readonly IManualTransfersHttpClient _manualTransfersHttpClient;

    public ManualTransfersController(IManualTransfersHttpClient manualTransfersHttpClient)
    {
        _manualTransfersHttpClient = manualTransfersHttpClient;
    }
    
    [HttpGet("")]
    [Authorize(Policy = "EmoneyWallet:ReadAll")]
    public async Task<ActionResult<PaginatedList<ManualTransferResponse>>> GetAllAsync([FromQuery] GetAllManualTransfersRequest request)
    {
        return await _manualTransfersHttpClient.GetAllManualTransfersAsync(request);
    }
    
    [HttpPost("")]
    [Authorize(Policy = "EmoneyWallet:Create")]
    public async Task CreateManualTransferAsync([FromBody] CreateManualTransferRequest request)
    {
        await _manualTransfersHttpClient.CreateManualTransfersAsync(request);
    }
}