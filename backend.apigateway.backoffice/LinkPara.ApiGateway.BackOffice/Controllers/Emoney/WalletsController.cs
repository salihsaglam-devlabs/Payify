using LinkPara.ApiGateway.BackOffice.Services.EMoney.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Controllers.Emoney
{
    public class WalletsController : ApiControllerBase
    {
        private readonly IWalletHttpClient _walletHttpClient;
        public WalletsController(IWalletHttpClient walletHttpClient)
        {
            _walletHttpClient = walletHttpClient;
        }

        [HttpGet("summary/{walletNumber}")]
        public async Task<WalletSummeryDto> GetWalletSummary([FromRoute] string walletNumber)
        {
            return await _walletHttpClient.GetWalletSummaryAsync(new GetWalletSummaryRequest { WalletNumber = walletNumber });
        }

        [HttpPatch("")]
        [Authorize(Policy = "EmoneyWallet:Update")]
        public async Task UpdateWalletAsync(UpdateWalletRequest request)
        {
            await _walletHttpClient.UpdateWalletAsync(request);
        }

        [HttpGet("walletBalance")]
        [Authorize(Policy = "EmoneyWallet:ReadAll")]
        public async Task<ActionResult<WalletBalanceResponse>> GetWalletBalancesAsync([FromQuery] GetWalletBalanceRequest query)
        {
            return await _walletHttpClient.GetWalletBalancesAsync(query);
        }


        [HttpGet("wallet-balances-daily")]
        [Authorize(Policy = "WalletBalanceDaily:ReadAll")]
        public async Task<ActionResult<WalletBalanceDailyResponse>> GetWalletBalanceDailyAsync([FromQuery] GetWalletBalancesDailyRequest query)
        {
            return await _walletHttpClient.GetWalletBalanceDailyAsync(query);
        }
    }
}
