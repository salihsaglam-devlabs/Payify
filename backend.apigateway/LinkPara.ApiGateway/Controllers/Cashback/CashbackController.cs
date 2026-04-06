using LinkPara.ApiGateway.Services.Cashback.HttpClients;
using LinkPara.ApiGateway.Services.Cashback.Models.Requests;
using LinkPara.ApiGateway.Services.Cashback.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.Controllers.Cashback;

public class CashbackController : ApiControllerBase
{
    private readonly ICashbackHttpClient _cashbackHttpClient;

    public CashbackController(ICashbackHttpClient cashbackHttpClient)
    {
        _cashbackHttpClient = cashbackHttpClient;
    }

    /// <summary>
    /// This is a method used to get filtered rule list.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "CashbackRule:ReadAll")]
    [HttpGet("")]
    public async Task<ActionResult<PaginatedList<CashbackRuleSummaryDto>>> GetFilteredRulesAsync([FromQuery] GetFilteredRulesRequest request)
    {
        return await _cashbackHttpClient.GetFilteredRulesAsync(request);
    }
}