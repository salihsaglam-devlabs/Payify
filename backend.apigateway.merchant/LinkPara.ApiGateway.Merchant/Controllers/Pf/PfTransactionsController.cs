using LinkPara.ApiGateway.Merchant.Services.Pf.HttpClients;
using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.Merchant.Controllers.Pf;

public class PfTransactionsController : ApiControllerBase
{
    private readonly IPfTransactionHttpClient _pfTransactionHttpClient;

    public PfTransactionsController(IPfTransactionHttpClient pfTransactionHttpClient)
    {
        _pfTransactionHttpClient = pfTransactionHttpClient;
    }

    /// <summary>
    /// Returns all transactions
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpGet("")]
    [Authorize(Policy = "PfTransaction:ReadAll")]
    public async Task<ActionResult<PaginatedList<PfTransactionDto>>> GetAllAsync([FromQuery] GetAllTransactionRequest request)
    {
        return await _pfTransactionHttpClient.GetAllAsync(request);
    }
}
