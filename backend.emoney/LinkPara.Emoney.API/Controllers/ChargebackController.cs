using LinkPara.Emoney.Application.Features.Chargebacks;
using LinkPara.Emoney.Application.Features.Chargebacks.Commands;
using LinkPara.Emoney.Application.Features.Chargebacks.Queries;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.Emoney.API.Controllers;

public class ChargebackController : ApiControllerBase
{
    /// <summary>
    /// Gets and Filters Chargeback Items
    /// </summary>
    /// <param name="query"></param>
    [Authorize(Policy = "Chargeback:ReadAll")]
    [HttpGet("")]
    public async Task<PaginatedList<ChargebackDto>> GetChargeback([FromQuery] GetChargebackQuery query)
    {
        return await Mediator.Send(query);
    }

    /// <summary>
    /// Initialize Chargeback Operation
    /// </summary>
    /// <returns></returns>
    [Authorize(Policy = "Chargeback:Create")]
    [HttpPost("init")]
    public async Task<ChargebackDto> InitializeChargeback([FromBody] InitChargebackCommand request)
    {
        return await Mediator.Send(request);
    }

    /// <summary>
    /// Approve Chargeback Operation
    /// </summary>
    /// <returns></returns>
    [Authorize(Policy = "Chargeback:Update")]
    [HttpPut("approve")]
    public async Task<ChargebackDto> ApproveChargeback([FromBody] ApproveChargebackCommand request)
    {
        return await Mediator.Send(request);
    }

    /// <summary>
    /// Creates Chargeback Document
    /// </summary>
    /// <returns></returns>
    [Authorize(Policy = "Chargeback:Create")]
    [HttpPost("add-document")]
    public async Task<ChargebackDocumentDto> AddChargebackDocument([FromBody] AddChargebackDocumentCommand request)
    {
        return await Mediator.Send(request);
    }

    /// <summary>
    /// Deletes Chargeback Document
    /// </summary>
    /// <returns></returns>
    [Authorize(Policy = "Chargeback:Update")]
    [HttpPut("delete-document")]
    public async Task<bool> DeleteChargebackDocument([FromBody] DeleteChargebackDocumentCommand request)
    {
        return await Mediator.Send(request);
    }

    /// <summary>
    /// Gets Chargeback Documents
    /// </summary>
    /// <returns></returns>
    [Authorize(Policy = "Chargeback:ReadAll")]
    [HttpGet("get-documents")]
    public async Task<List<ChargebackDocumentDto>> GetChargebackDocuments([FromQuery] GetChargebackDocumentQuery request)
    {
        return await Mediator.Send(request);
    }
}