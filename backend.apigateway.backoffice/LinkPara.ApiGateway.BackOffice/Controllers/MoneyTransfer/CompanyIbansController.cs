using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Controllers.MoneyTransfer;

public class CompanyIbansController : ApiControllerBase
{
    private readonly ICompanyIbanHttpClient _client;

    public CompanyIbansController(ICompanyIbanHttpClient client)
    {
        _client = client;
    }

    /// <summary>
    /// Returns a company iban number.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [Authorize(Policy = "CompanyIban:Read")]
    public async Task<CompanyIbanDto> GetByIdAsync([FromRoute] Guid id)
    {
        return await _client.GetByIdAsync(id);
    }

    /// <summary>
    /// Returns all company iban numbers.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpGet("")]
    [Authorize(Policy = "CompanyIban:ReadAll")]
    public async Task<PaginatedList<CompanyIbanDto>> GetListAsync([FromQuery] GetCompanyIbanListRequest request)
    {
        return await _client.GetListAsync(request);
    }

    /// <summary>
    /// Creates a new company iban.
    /// </summary>
    /// <param name="request"></param>    
    /// <returns></returns>
    [HttpPost("")]
    [Authorize(Policy = "CompanyIban:Create")]
    public async Task SaveAsync(SaveCompanyIbanRequest request)
    {
        await _client.SaveAsync(request);
    }

    /// <summary>
    /// Updates a company iban numbers.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPut("")]
    [Authorize(Policy = "CompanyIban:Update")]
    public async Task UpdateAsync(UpdateCompanyIbanRequest request)
    {
        await _client.UpdateAsync(request);
    }

    /// <summary>
    /// Delete company iban numbers.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpDelete("")]
    [Authorize(Policy = "CompanyIban:Delete")]
    public async Task DeleteAsync(DeleteCompanyIbanRequest request)
    {
        await _client.DeleteAsync(request);
    }
}
