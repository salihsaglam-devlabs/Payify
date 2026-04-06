using LinkPara.ApiGateway.CorporateWallet.Services.Emoney.HttpClients;
using LinkPara.ApiGateway.CorporateWallet.Services.Emoney.Models.Requests;
using LinkPara.ApiGateway.CorporateWallet.Services.Emoney.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.CorporateWallet.Controllers.Emoney;

public class CorporateWalletsController : ApiControllerBase
{
    private readonly ICorporateWalletHttpClient _httpClient;

    public CorporateWalletsController(ICorporateWalletHttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// add account user
    /// </summary>
    /// <returns></returns>
    [HttpPost("users")]
    [Authorize(Policy = "CorporateWalletUser:Create")]
    public async Task AddUserAsync([FromBody] AddCorporateWalletUserRequest request)
    {
        await _httpClient.AddCorporateWalletUserAsync(request);
    }

    /// <summary>
    /// update account user
    /// </summary>
    /// <returns></returns>
    [HttpPut("users")]
    [Authorize(Policy = "CorporateWalletUser:Update")]
    public async Task UpdateUserAsync([FromBody] UpdateCorporateWalletUserRequest request)
    {
        await _httpClient.UpdateCorporateWalletUserAsync(request);
    }

    /// <summary>
    /// delete account user
    /// </summary>
    /// <returns></returns>
    [HttpPut("users/deactivate")]
    [Authorize(Policy = "CorporateWalletUser:Delete")]
    public async Task DeactivateUserAsync([FromBody] DeactivateCorporateWalletUserRequest request)
    {
        await _httpClient.DeactivateCorporateWalletUserAsync(request);
    }

    /// <summary>
    /// update account user
    /// </summary>
    /// <returns></returns>
    [HttpPut("users/activate")]
    [Authorize(Policy = "CorporateWalletUser:Update")]
    public async Task ActivateUserAsync([FromBody] ActivateCorporateWalletUserRequest request)
    {
        await _httpClient.ActivateCorporateWalletUserAsync(request);
    }

    /// <summary>
    /// get account users
    /// </summary>
    /// <returns></returns>
    [HttpGet("users")]
    [Authorize(Policy = "CorporateWalletUser:ReadAll")]
    public async Task<PaginatedList<CorporateWalletUserDto>> GetUsersAsync([FromQuery] GetCorporateWalletUsersRequest request)
    {
        return await _httpClient.GetCorporateWalletUsersAsync(request);
    }

    /// <summary>
    /// get account user
    /// </summary>
    /// <returns></returns>
    [HttpGet("users/{id}")]
    [Authorize(Policy = "CorporateWalletUser:Read")]
    public async Task<CorporateWalletUserDto> GetUserByIdAsync([FromRoute] Guid id)
    {
        return await _httpClient.GetCorporateWalletUserAsync(id);
    }

    /// <summary>
    /// get Corporate account
    /// </summary>
    /// <returns></returns>
    [HttpGet("accounts/me")]
    [Authorize(Policy = "CorporateWalletAccount:Read")]
    public async Task<CorporateAccountDto> GetAccountByIdAsync()
    {
        return await _httpClient.GetCorporateWalletAccountAsync();
    }

}
