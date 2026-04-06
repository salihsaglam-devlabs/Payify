using LinkPara.ApiGateway.BackOffice.Services.EMoney.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;

namespace LinkPara.ApiGateway.BackOffice.Controllers.Emoney;

public class TierLevelsController : ApiControllerBase
{
    private readonly ITierLevelHttpClient _tierLevelHttpClient;
    
    public TierLevelsController(ITierLevelHttpClient tierLevelHttpClient)
    {
        _tierLevelHttpClient = tierLevelHttpClient;
    }
    
    /// <summary>
    /// Returns tier levels.
    /// </summary>
    /// <returns></returns>
    [HttpGet("")]
    [Authorize(Policy = "Limit:ReadAll")]
    public async Task<List<TierLevelResponse>> GetTierLevelsAsync([FromQuery] GetTierLevelsRequest request)
    {
        return await _tierLevelHttpClient.GetTierLevelsAsync(request);
    }

    /// <summary>
    /// Returns tier level by id.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [Authorize(Policy = "Limit:ReadAll")]
    public async Task<TierLevelResponse> GetTierLevelByIdAsync(Guid id)
    {
        return await _tierLevelHttpClient.GetTierLevelByIdAsync(id);
    }
    
    /// <summary>
    /// Creates a new custom limit level.
    /// </summary>
    /// <returns></returns>
    [HttpPost("")]
    [Authorize(Policy = "Limit:Create")]
    public async Task CreateTierLevelsAsync(CustomTierLevelDto request)
    {
        await _tierLevelHttpClient.CreateTierLevelsAsync(request);
    }

    /// <summary>
    /// Partial updates custom tier level.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPatch("{id}")]
    [Authorize(Policy = "Limit:Update")]
    public async Task PatchCustomTierLevelAsync(Guid id, [FromBody] JsonPatchDocument<CustomTierLevelDto> request)
    {
        await _tierLevelHttpClient.PatchCustomTierLevelAsync(id, request);
    }

    /// <summary>
    /// Disables custom tier level.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    [Authorize(Policy = "Limit:Delete")]
    public async Task DisableCustomTierLevelAsync(Guid id)
    {
        await _tierLevelHttpClient.DisableCustomTierLevelAsync(id);
    }

    /// <summary>
    /// Creates a new account custom limit level.
    /// </summary>
    /// <returns></returns>
    [HttpPost("account-tier-level")]
    [Authorize(Policy = "Limit:Create")]
    public async Task CreateAccountCustomTierAsync(AccountCustomTierDto request)
    {
        await _tierLevelHttpClient.CreateAccountCustomTierAsync(request);
    }

    /// <summary>
    /// Disables custom tier level.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("custom-tier-level/{id}")]
    [Authorize(Policy = "Limit:Delete")]
    public async Task DeleteAccountCustomTierAsync(Guid id)
    {
        await _tierLevelHttpClient.DeleteAccountCustomTierAsync(id);
    }
}