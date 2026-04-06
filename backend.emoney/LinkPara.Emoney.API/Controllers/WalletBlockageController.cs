using LinkPara.Emoney.Application.Features.WalletBlockages;
using LinkPara.Emoney.Application.Features.WalletBlockages.Commands;
using LinkPara.Emoney.Application.Features.WalletBlockages.Queries;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.Emoney.API.Controllers;

public class WalletBlockageController : ApiControllerBase
{
    /// <summary>
    /// Gets and Filters WalletBlockage Items
    /// </summary>
    /// <param name="query"></param>
    [Authorize(Policy = "WalletBlockage:ReadAll")]
    [HttpGet("get-blockages")]
    public async Task<PaginatedList<WalletBlockageDto>> GetWalletBlockageAsync([FromQuery] GetWalletBlockageQuery query)
    {
        return await Mediator.Send(query);
    }

    /// <summary>
    /// Insert a WalletBlockage Operation
    /// </summary>
    /// <returns></returns>
    [Authorize(Policy = "WalletBlockage:Create")]
    [HttpPost("wallet-blockage")]
    public async Task WalletBlockageRequestAsync([FromBody] AddWalletBlockageCommand request)
    {
        await Mediator.Send(request);
    }

    /// <summary>
    /// Remove WalletBlockage Operation
    /// </summary>
    /// <returns></returns>
    [Authorize(Policy = "WalletBlockage:Update")]
    [HttpPut("remove-blockage-batch")]
    public async Task<List<WalletBlockage>> RemoveExpiredBlockagesAsync(RemoveWalletBlockageCommand request)
    {
        return await Mediator.Send(request);
    }

    /// <summary>
    /// Creates WalletBlockage Document
    /// </summary>
    /// <returns></returns>
    [Authorize(Policy = "WalletBlockage:Create")]
    [HttpPost("add-document")]
    public async Task<WalletBlockageDocumentDto> AddWalletBlockageDocumentAsync([FromBody] AddWalletBlockageDocumentCommand request)
    {
        return await Mediator.Send(request);
    }

    /// <summary>
    /// Deletes WalletBlockage Document
    /// </summary>
    /// <returns></returns>
    [Authorize(Policy = "WalletBlockage:Update")]
    [HttpPut("remove-document")]
    public async Task<bool> RemoveWalletBlockageDocumentAsync([FromBody] RemoveWalletBlockageDocumentCommand request)
    {
        return await Mediator.Send(request);
    }

    /// <summary>
    /// Gets WalletBlockage Documents
    /// </summary>
    /// <returns></returns>
    [Authorize(Policy = "WalletBlockage:Read")]
    [HttpGet("get-documents")]
    public async Task<List<WalletBlockageDocumentDto>> GetWalletBlockageDocumentsAsync([FromQuery] GetWalletBlockageDocumentQuery request)
    {
        return await Mediator.Send(request);
    }
}