using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Controllers.MoneyTransfer;

public class BankApisController : ApiControllerBase
{
    private readonly IBankApiHttpClient _client;

    public BankApisController(IBankApiHttpClient client)
    {
        _client = client;
    }

    /// <summary>
    /// Returns bank api of company.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [Authorize(Policy = "BankApi:Read")]
    public async Task<BankApiDto> GetByIdAsync([FromRoute] Guid id)
    {
        return await _client.GetByIdAsync(id);
    }

    /// <summary>
    /// Returns bank apis of company.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpGet("")]
    [Authorize(Policy = "BankApi:ReadAll")]
    public async Task<PaginatedList<BankApiDto>> GetListAsync([FromQuery] GetBankApiListRequest request)
    {
        return await _client.GetListAsync(request);
    }

    /// <summary>
    /// Saves bank api for company. Time formats must be hh:mm:ss
    /// </summary>
    /// <param name="request"></param>    
    /// <returns></returns>
    [HttpPost("")]
    [Authorize(Policy = "BankApi:Create")]
    public async Task SaveAsync(SaveBankApiRequest request)
    {
        await _client.SaveAsync(request);
    }

    /// <summary>
    /// Updates bank api for company. Time formats must be hh:mm:ss
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPut("")]
    [Authorize(Policy = "BankApi:Update")]
    public async Task UpdateAsync(UpdateBankApiRequest request)
    {
        await _client.UpdateAsync(request);
    }

    /// <summary>
    /// Deletes bank api for company.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpDelete("")]
    [Authorize(Policy = "BankApi:Delete")]
    public async Task DeleteAsync(DeleteBankApiRequest request)
    {
        await _client.DeleteAsync(request);
    }
}
