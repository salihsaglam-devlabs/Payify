using LinkPara.PF.Application.Features.MerchantIntegrators;
using LinkPara.PF.Application.Features.MerchantIntegrators.Command.DeleteMerchantIntegrator;
using LinkPara.PF.Application.Features.MerchantIntegrators.Command.SaveMerchantIntegrator;
using LinkPara.PF.Application.Features.MerchantIntegrators.Command.UpdateMerchantIntegrator;
using LinkPara.PF.Application.Features.MerchantIntegrators.Queries.GetAllMerchantIntegrator;
using LinkPara.PF.Application.Features.MerchantIntegrators.Queries.GetMerchantIntegratorById;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.PF.API.Controllers;

public class MerchantIntegratorsController : ApiControllerBase
{
    /// <summary>
    /// Returns a merchant integrator
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize(Policy = "MerchantIntegrator:Read")]
    [HttpGet("{id}")]
    public async Task<ActionResult<MerchantIntegratorDto>> GetByIdAsync([FromRoute] Guid id)
    {
        return await Mediator.Send(new GetMerchantIntegratorByIdQuery { Id = id });
    }

    /// <summary>
    /// Returns all merchant integrators
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "MerchantIntegrator:ReadAll")]
    [HttpGet("")]
    public async Task<PaginatedList<MerchantIntegratorDto>> GetAllAsync([FromQuery] SearchQueryParams request)
    {
        return await Mediator.Send(new GetAllMerchantIntegratorQuery { SearchQueryParams = request });
    }

    /// <summary>
    /// Creates a merchant integrator
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [Authorize(Policy = "MerchantIntegrator:Create")]
    [HttpPost("")]
    public async Task SaveAsync(SaveMerchantIntegratorCommand command)
    {
        await Mediator.Send(command);
    }

    /// <summary>
    /// Updates a merchant integrator
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [Authorize(Policy = "MerchantIntegrator:Update")]
    [HttpPut("")]
    public async Task UpdateAsync(UpdateMerchantIntegratorCommand command)
    {
        await Mediator.Send(command);
    }

    /// <summary>
    /// Delete a merchant integrator
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize(Policy = "MerchantIntegrator:Delete")]
    [HttpDelete("{id}")]
    public async Task DeleteAsync(Guid id)
    {
        await Mediator.Send(new DeleteMerchantIntegratorCommand { Id = id });
    }
}
