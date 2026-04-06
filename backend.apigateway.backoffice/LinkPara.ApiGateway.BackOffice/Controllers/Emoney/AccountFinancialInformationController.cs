using LinkPara.ApiGateway.BackOffice.Services.EMoney.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Controllers.Emoney
{
    public class AccountFinancialInformationController : ApiControllerBase
    {
        private readonly IAccountFinancialInfoHttpClient _httpClient;
        public AccountFinancialInformationController(IAccountFinancialInfoHttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        /// <summary>
        /// Returns an Account Financial Information by account id
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns></returns>
        [HttpGet("{accountId}")]
        [Authorize(Policy = "AccountFinancialInformation:Read")]
        public async Task<AccountFinancialInfoDto> GetAccountFinancialInfoByAccountId(Guid accountId)
        {
            return await _httpClient.GetAccountFinancialInfoAsync(accountId);
        }
    }
}
