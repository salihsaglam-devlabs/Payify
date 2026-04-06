using LinkPara.ApiGateway.Services.Emoney.HttpClients;
using LinkPara.ApiGateway.Services.Emoney.Models.Responses;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.Controllers.Emoney;

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
    [Authorize(Policy ="Bank:ReadAll")]
    [HttpGet("")]
    public async Task<List<SystemBankAccountDto>> GetSystemBankAccountDetailsAsync()
    {
        return await _systemBankAccountHttpClient.GetSystemBankAccountsAsync();
    }
}