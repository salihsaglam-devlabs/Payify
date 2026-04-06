using LinkPara.ApiGateway.Boa.Filters.CustomerContext;
using LinkPara.ApiGateway.Boa.Services.MoneyTransfer.HttpClients;
using LinkPara.ApiGateway.Boa.Services.MoneyTransfer.Models.Requests;
using LinkPara.ApiGateway.Boa.Services.MoneyTransfer.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.Boa.Controllers.MoneyTransfer;

public class SourceBankAccountsController : ApiControllerBase
{
    private readonly ISourceBankAccountHttpClient _client;

    public SourceBankAccountsController(ISourceBankAccountHttpClient client)
    {
        _client = client;
    }

    /// <summary>
    /// Returns all accounts.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpGet("")]
    [CustomerContextRequired]
    public async Task<PaginatedList<SourceBankAccountDto>> GetListAsync(
        [FromQuery] GetSourceBankAccountListRequest request)
    {
        return await _client.GetListAsync(request);
    }
}
