using Microsoft.AspNetCore.Mvc;
using LinkPara.Identity.Application.Features.Address.Queries;
using LinkPara.Identity.Application.Features.Address.Queries.GetAddressByUserId;
using LinkPara.Identity.Application.Features.Address.Commands.CreateUserAddress;
using LinkPara.Identity.Application.Features.Address.Commands.UpdateUserAddress;
using LinkPara.Identity.Application.Features.Address.Commands.DeleteUserAddress;
using Microsoft.AspNetCore.Authorization;

namespace LinkPara.Identity.API.Controllers;

public class UserAddressesController : ApiControllerBase
{
    [Authorize(Policy = "UserAddress:ReadAll")]
    [HttpGet("{userId}")]
    public async Task<ActionResult<List<UserAddressDto>>> GetAddressByUserIdAsync(Guid userId)
    {
        return await Mediator.Send(new GetAddressByUserIdQuery { UserId = userId });
    }

    [Authorize(Policy = "UserAddress:ReadCreate")]
    [HttpPost]
    public async Task<ActionResult> CreateAddressAsync(CreateAddressCommand query)
    {
        await Mediator.Send(query);

        return NoContent();
    }
    [Authorize(Policy = "UserAddress:Update")]
    [HttpPut("{addressId}")]
    public async Task<ActionResult> UpdateAsync(Guid addressId, [FromBody]UpdateAddressCommand command)
    {
        if (addressId != command.Id)
        {
            return BadRequest();
        }

        await Mediator.Send(command);

        return NoContent();
    }
    [Authorize(Policy = "UserAddress:Delete")]
    [HttpDelete("{addressId}")]
    public async Task<ActionResult> DeleteAsync(Guid addressId)
    {
        await Mediator.Send(new DeleteAddressCommand { Id = addressId });

        return NoContent();
    }
}