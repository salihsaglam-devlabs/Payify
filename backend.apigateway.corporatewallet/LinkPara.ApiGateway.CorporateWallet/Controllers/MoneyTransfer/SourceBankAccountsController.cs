using LinkPara.ApiGateway.CorporateWallet.Services.MoneyTransfer.HttpClients;
using LinkPara.ApiGateway.CorporateWallet.Services.MoneyTransfer.Models.Requests;
using LinkPara.ApiGateway.CorporateWallet.Services.MoneyTransfer.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.CorporateWallet.Controllers.MoneyTransfer
{
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
        [Authorize(Policy = "SourceBankAccount:ReadAll")]
        [HttpGet("")]
        public async Task<PaginatedList<SourceBankAccountDto>> GetListAsync([FromQuery] GetSourceBankAccountListRequest request)
        {
            return await _client.GetListAsync(request);
        }

    }
}
