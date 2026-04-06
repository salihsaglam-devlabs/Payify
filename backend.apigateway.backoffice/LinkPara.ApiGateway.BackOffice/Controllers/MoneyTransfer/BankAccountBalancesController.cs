using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Controllers.MoneyTransfer;

public class BankAccountBalancesController : ApiControllerBase
{
    private readonly IBankAccountBalancesHttpClient _client;

    public BankAccountBalancesController(IBankAccountBalancesHttpClient client)
    {
        _client = client;
    }

    /// <summary>
    /// returns bank account balances
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "BankAccountBalances:ReadAll")]
    [HttpGet("")]
    public async Task<PaginatedList<BankAccountBalanceDto>> GetReconciliationDetailsAsync([FromQuery] BankAccountBalanceRequest request)
    {
        return await _client.GetBankAccountBalancesAsync(request);
    }
}
