using LinkPara.ApiGateway.CorporateWallet.Services.Emoney.HttpClients;
using LinkPara.ApiGateway.CorporateWallet.Services.Emoney.Models.Requests;
using LinkPara.ApiGateway.CorporateWallet.Services.Emoney.Models.Responses;
using LinkPara.SharedModels.Pagination;
using MassTransit.Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.CorporateWallet.Controllers.Emoney;

public class BulkTransfersController : ApiControllerBase
{
    private readonly IBulkTransferHttpClient _bulkTransferHttpClient;

    public BulkTransfersController(IBulkTransferHttpClient bulkTransferHttpClient)
    {
        _bulkTransferHttpClient = bulkTransferHttpClient;
    }

    /// <summary>
    /// save bulk transfer
    /// </summary>
    /// <returns></returns>
    [HttpPost("")]
    [Authorize(Policy = "BulkTransfer:Create")]
    public async Task SaveAsync([FromBody] SaveBulkTransferRequest request)
    {
        await _bulkTransferHttpClient.SaveBulkTransferAsync(request);
    }

    /// <summary>
    /// get bulk transfers
    /// </summary>
    /// <returns></returns>
    [HttpGet("")]
    [Authorize(Policy = "BulkTransfer:ReadAll")]
    public async Task<PaginatedList<BulkTransferDto>> GetListAsync([FromQuery] GetBulkTransfersRequest request)
    {
        return await _bulkTransferHttpClient.GetBulkTransfersAsync(request);
    }

    /// <summary>
    /// get bulk transfer
    /// </summary>
    /// <returns></returns>
    [HttpGet("{id}")]
    [Authorize(Policy = "BulkTransfer:Read")]
    public async Task<BulkTransferDto> GetByIdAsync([FromRoute] Guid id)
    {
        return await _bulkTransferHttpClient.GetBulkTransferByIdAsync(id);
    }


    /// <summary>
    /// action bulk transfer
    /// </summary>
    /// <returns></returns>
    [HttpPut("action")]
    [Authorize(Policy = "BulkTransfer:Update")]
    public async Task ActionBulkTransferAsync([FromBody] ActionBulkTransferRequest request)
    {
        await _bulkTransferHttpClient.ActionBulkTransferAsync(request);
    }

    /// <summary>
    /// get report bulk transfers
    /// </summary>
    /// <returns></returns>
    [HttpGet("report")]
    [Authorize(Policy = "BulkTransferReport:ReadAll")]
    public async Task<PaginatedList<BulkTransferDto>> GetReportListAsync([FromQuery] GetReportBulkTransferRequest request)
    {
        return await _bulkTransferHttpClient.GetReportBulkTransfersAsync(request);
    }

    /// <summary>
    /// get report bulk transfer
    /// </summary>
    /// <returns></returns>
    [HttpGet("report/{id}")]
    [Authorize(Policy = "BulkTransferReport:Read")]
    public async Task<BulkTransferDto> GetReportByIdAsync([FromRoute] Guid id)
    {
        return await _bulkTransferHttpClient.GetReportBulkTransferByIdAsync(id);
    }
}
