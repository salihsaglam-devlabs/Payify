using LinkPara.ApiGateway.BackOffice.Services.EMoney.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Controllers.Emoney;

public class MasterpassController : ApiControllerBase
{
    private readonly IMasterpassHttpClient _httpClient;

    public MasterpassController(IMasterpassHttpClient httpClient)
        => _httpClient = httpClient;

    /// <summary>
    /// Topup cancel
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "Topup:Create")]
    [HttpPost("cancel")]
    public async Task<TopupCancelResponse> TopupCancelAsync([FromBody] MasterpassCancelRequest request)
        => await _httpClient.TopupCancelAsync(request);
}