using LinkPara.ApiGateway.Merchant.Services.Pf.HttpClients;
using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.Merchant.Controllers.Pf;

public class MerchantTransactionsController : ApiControllerBase
{
    private readonly IMerchantTransactionHttpClient _pfTransactionHttpClient;

    public MerchantTransactionsController(IMerchantTransactionHttpClient pfTransactionHttpClient)
    {
        _pfTransactionHttpClient = pfTransactionHttpClient;
    }

    /// <summary>
    /// Returns all transactions
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpGet("")]
    [Authorize(Policy = "MerchantTransaction:ReadAll")]
    public async Task<ActionResult<PaginatedList<MerchantTransactionDto>>> GetAllAsync([FromQuery] GetAllMerchantTransactionRequest request)
    {
        return await _pfTransactionHttpClient.GetAllAsync(request);
    }

    /// <summary>
    /// Return a transaction
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [Authorize(Policy = "MerchantTransaction:Read")]
    public async Task<ActionResult<MerchantTransactionDto>> GetByIdAsync([FromRoute] Guid id)
    {
        return await _pfTransactionHttpClient.GetByIdAsync(id);
    }

    /// <summary>
    /// Return a transaction status counts
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpGet("statusCount")]
    [Authorize(Policy = "MerchantTransaction:Read")]
    public async Task<ActionResult<List<MerchantTransactionStatusModel>>> GetStatusCountAsync([FromQuery] MerchantTransactionStatusRequest request)
    {
        return await _pfTransactionHttpClient.GetStatusCountAsync(request);
    }

    /// <summary>
    /// Generate order number
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "MerchantTransaction:Read")]
    [HttpPost("generate-orderNumber")]
    public async Task<ActionResult<OrderNumberResponse>> GenerateUniqueOrderNumberAsync(OrderNumberRequest request)
    {
        return await _pfTransactionHttpClient.GenerateUniqueOrderNumberAsync(request);
    }
}
