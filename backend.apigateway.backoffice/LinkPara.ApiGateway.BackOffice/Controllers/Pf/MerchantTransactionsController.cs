using LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Controllers.Pf;

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
    /// Updates merchant transaction with patch
    /// </summary>
    /// <param name="id"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPatch("{id}")]
    [Authorize(Policy = "MerchantTransaction:Update")]
    public async Task PatchAsync(Guid id, [FromBody] JsonPatchDocument<UpdateMerchantTransactionRequest> request)
    {
        await _pfTransactionHttpClient.PatchAsync(id, request);
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
    /// <param name="merchantId"></param>
    /// <returns></returns>
    [HttpGet("{merchantId}/generate-orderNumber")]
    public async Task<ActionResult<string>> GenerateOrderNumberAsync([FromRoute] Guid merchantId)
    {
        return await _pfTransactionHttpClient.GenerateOrderNumberAsync(merchantId);
    }
    
    
    /// <summary>
    /// Manual Return order
    /// </summary>
    /// <returns></returns>
    [Authorize(Policy = "MerchantTransaction:Read")]
    [HttpPost("manual-return")]
    public async Task ReturnAsync(ManualReturnRequest request)
    {
        await _pfTransactionHttpClient.ManualReturnAsync(request);
    }


    /// <summary>
    /// Returns all installment transactions
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpGet("merchantInstallmentTransactions")]
    [Authorize(Policy = "MerchantTransaction:ReadAll")]
    public async Task<ActionResult<PaginatedList<MerchantInstallmentTransactionDto>>> GetAllInstallmentTransactionAsync([FromQuery] GetAllMerchantInstallmentTransactionRequest request)
    {
        return await _pfTransactionHttpClient.GetAllInstallmentTransactionAsync(request);
    }
}
