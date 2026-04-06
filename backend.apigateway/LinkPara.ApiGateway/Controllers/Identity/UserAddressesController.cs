using LinkPara.ApiGateway.Services.Identity.HttpClients;
using LinkPara.ApiGateway.Services.Identity.Models.Requests;
using LinkPara.ApiGateway.Services.Identity.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.Controllers.Identity;

public class UserAddressesController : ApiControllerBase
{
    private readonly IUserAddressHttpClient _userAddressHttpClient;

    public UserAddressesController(IUserAddressHttpClient userAddressHttpClient)
    {
        _userAddressHttpClient = userAddressHttpClient;
    }

    /// <summary>
    /// Returns address informations of the logged in user.
    /// </summary>
    [Authorize(Policy= "UserAddress:Read")]
    [HttpGet("me")]
    public async Task<ActionResult<List<UserAddressDto>>> GetMyAddressesAsync()
    {
        return await _userAddressHttpClient.GetAddressByUserIdAsync(UserId);
    }

    /// <summary>
    /// Creates a new address for the logged in user.
    /// </summary>
    /// <param name="userAddress"></param>
    [Authorize(Policy = "UserAddress:Create")]
    [HttpPost("")]
    public async Task PostAsync(CreateAddressRequest userAddress)
    {
        await _userAddressHttpClient.CreateAddressAsync(userAddress);
    }

    /// <summary>
    /// Updates the users address.
    /// </summary>
    /// <param name="userAddress"></param>
    [Authorize(Policy = "UserAddress:Update")]
    [HttpPut("")]
    public async Task UpdateAsync(UpdateAddressRequest userAddress)
    {
        await _userAddressHttpClient.UpdateAddressAsync(userAddress);
    }
}
