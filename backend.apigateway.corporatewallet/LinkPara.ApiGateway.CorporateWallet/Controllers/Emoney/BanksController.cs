using LinkPara.ApiGateway.CorporateWallet.Services.Emoney.HttpClients;
using LinkPara.ApiGateway.CorporateWallet.Services.Emoney.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.CorporateWallet.Controllers.Emoney;

public class BanksController : ApiControllerBase
{
    private readonly IBankHttpClient _bankHttpClient;

    public BanksController(IBankHttpClient bankHttpClient)
    {
        _bankHttpClient = bankHttpClient;
    }

    /// <summary>
    /// Returns all bank list
    /// </summary>
    /// <param name="iban"></param>
    /// <returns></returns>
    [Authorize(Policy = "Bank:ReadAll")]
    [HttpGet("")]
    public async Task<ActionResult<List<BankDto>>> GetBanksAsync(string iban = null)
    {
        return await _bankHttpClient.GetBanksAsync(iban);
    }
}