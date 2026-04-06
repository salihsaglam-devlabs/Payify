using LinkPara.Emoney.Application.Features.CommercialPricing;
using LinkPara.Emoney.Application.Features.CommercialPricing.Commands.CreatePricingCommercial;
using LinkPara.Emoney.Application.Features.CommercialPricing.Commands.DeletePricingCommercial;
using LinkPara.Emoney.Application.Features.CommercialPricing.Commands.UpdatePricingCommercial;
using LinkPara.Emoney.Application.Features.CommercialPricing.Queries.GetAllCommercialPricingHistory;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.Emoney.API.Controllers;

public class CommercialPricingController : ApiControllerBase
{
    /// <summary>
    /// Returns all commercial pricings
    /// </summary>
    /// <returns></returns>
    [Authorize(Policy = "CommercialPricing:ReadAll")]
    [HttpGet("all")]
    public async Task<PaginatedList<PricingCommercialDto>> GetAll([FromQuery] CommercialPricingQuery request)
    {
        return await Mediator.Send(request);
    }
    
    /// <summary>
    /// Adds new Commercial pricing record
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [Authorize(Policy = "CommercialPricing:Create")]
    [HttpPost("create")]
    public async Task CreatePricingCommercial(CreatePricingCommercialCommand command)
    {
        await Mediator.Send(command);
    }
    
    /// <summary>
    /// Updates commercial pricing record
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [Authorize(Policy = "CommercialPricing:Update")]
    [HttpPut("update")]
    public async Task UpdatePricingCommercial(UpdateCommercialPricingCommand command)
    {
        await Mediator.Send(command);
    }
    
    /// <summary>
    /// Set record's status to passive
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize(Policy = "CommercialPricing:Delete")]
    [HttpDelete("{id}")]
    public async Task DeletePricingCommercial(Guid id)
    {
        await Mediator.Send(new DeletePricingCommercialCommand { Id = id });
    }
}