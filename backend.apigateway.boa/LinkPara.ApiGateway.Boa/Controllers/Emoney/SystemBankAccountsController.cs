using LinkPara.ApiGateway.Boa.Filters.CustomerContext;
using LinkPara.ApiGateway.Boa.Services.Emoney.HttpClients;
using LinkPara.ApiGateway.Boa.Services.Emoney.Models.Responses;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.Boa.Controllers.Emoney;

public class SystemBankAccountsController : ApiControllerBase
{
    private readonly ISystemBankAccountHttpClient _systemBankAccountHttpClient;

    public SystemBankAccountsController(ISystemBankAccountHttpClient systemBankAccountHttpClient)
    {
        _systemBankAccountHttpClient = systemBankAccountHttpClient;
    }

    /// <summary>
    /// Returns system bank accounts.
    /// </summary>
    [HttpGet("")]
    [CustomerContextRequired]
    public async Task<List<SystemBankAccountDto>> GetSystemBankAccountDetailsAsync()
    {
        return await _systemBankAccountHttpClient.GetSystemBankAccountsAsync();
    }
}