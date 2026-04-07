using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Application.Commons.Models;
using LinkPara.Emoney.Application.Commons.Models.WalletModels;
using LinkPara.Emoney.Application.Features.Transactions;
using LinkPara.Emoney.Application.Features.Transactions.Queries.GetWalletTransactions;
using LinkPara.Emoney.Application.Features.Wallets;
using LinkPara.Emoney.Application.Features.Wallets.Commands.ConvertUserWalletsToIndividual;
using LinkPara.Emoney.Application.Features.Wallets.Commands.SaveWallet;
using LinkPara.Emoney.Application.Features.Wallets.Commands.Transfer;
using LinkPara.Emoney.Application.Features.Wallets.Commands.UpdateBalance;
using LinkPara.Emoney.Application.Features.Wallets.Commands.UpdateUserWallets;
using LinkPara.Emoney.Application.Features.Wallets.Commands.UpdateWallet;
using LinkPara.Emoney.Application.Features.Wallets.Commands.WithdrawRequests;
using LinkPara.Emoney.Application.Features.Wallets.Queries;
using LinkPara.Emoney.Application.Features.Wallets.Queries.GetAccountWallets;
using LinkPara.Emoney.Application.Features.Wallets.Queries.GetUserWallets;
using LinkPara.Emoney.Application.Features.Wallets.Queries.GetWalletBalanceDaily;
using LinkPara.Emoney.Application.Features.Wallets.Queries.GetWalletBalances;
using LinkPara.Emoney.Application.Features.Wallets.Queries.GetWalletDetails;
using LinkPara.Emoney.Application.Features.Wallets.Queries.GetWalletSummaries;
using LinkPara.Emoney.Application.Features.Wallets.Queries.TransferPreview;
using LinkPara.Emoney.Application.Features.Wallets.Queries.WithdrawPreview;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.Emoney.API.Controllers;

public class WalletsController : ApiControllerBase
{    
    /// <summary>
    /// Returns the details of a wallet.
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    [Authorize(Policy = "EmoneyWallet:Read")]
    [HttpGet("details")]
    public async Task<ActionResult<WalletDto>> GetWalletDetailsAsync([FromQuery] GetWalletDetailsQuery query)
    {
        return await Mediator.Send(query);
    }

    /// <summary>
    /// Returns the users wallets.
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    [Authorize(Policy = "EmoneyWallet:ReadAll")]
    [HttpGet("")]
    public async Task<ActionResult<List<WalletDto>>> GetUserWalletsAsync([FromQuery] GetUserWalletsFilterQuery query)
    {
        return await Mediator.Send(query);
    }

    /// <summary>
    /// Returns the summary of wallet.
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    [Authorize(Policy = "EmoneyWallet:Read")]
    [HttpGet("summary")]
    public async Task<ActionResult<WalletSummaryDto>> GetWalletSummaryAsync([FromQuery] GetWalletSummaryQuery query)
    {
        return await Mediator.Send(query);
    }

    /// <summary>
    /// Returns the wallet balances
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    [Authorize(Policy = "EmoneyWallet:ReadAll")]
    [HttpGet("walletBalance")]
    public async Task<ActionResult<WalletBalanceResponse>> GetWalletBalancesAsync([FromQuery] GetWalletBalancesQuery query)
    {
        return await Mediator.Send(query);
    }

    /// <summary>
    /// Creates new wallet for user.
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [Authorize(Policy = "EmoneyWallet:Create")]
    [HttpPost("")]
    public async Task SaveWalletAsync(SaveWalletCommand command)
    {
        await Mediator.Send(command);
    }

    /// <summary>
    /// Update wallet.
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [Authorize(Policy = "EmoneyWallet:Update")]
    [HttpPatch("")]
    public async Task UpdateWalletAsync(UpdateWalletCommand command)
    {
        await Mediator.Send(command);
    }

    /// <summary>
    /// Converts user wallets to individual.
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [Authorize(Policy = "EmoneyWallet:Update")]
    [HttpPost("convert-user-wallets-to-individual")]
    public async Task ConvertUserWalletsToIndividualAsync(ConvertUserWalletsToIndividualCommand command)
    {
        await Mediator.Send(command);
    }

    /// <summary>
    /// Transfers money between users.
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [Authorize(Policy = "EmoneyWallet:Create")]
    [HttpPost("transfer")]
    public async Task<MoneyTransferResponse> TransferAsync(TransferCommand command)
    {
        return await Mediator.Send(command);
    }

    /// <summary>
    /// Transfer request preview
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    [Authorize(Policy = "EmoneyWallet:Read")]
    [HttpGet("transfer-preview")]
    public async Task<TransferPreviewResponse> TransferPreviewAsync([FromQuery] TransferPreviewQuery query)
    {
        return await Mediator.Send(query);
    }

    /// <summary>
    /// Witdraw request preview.
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    [Authorize(Policy = "EmoneyWallet:Read")]
    [HttpGet("withdraw-preview")]
    public async Task<WithdrawPreviewResponse> WithdrawPreviewAsync([FromQuery] WithdrawPreviewQuery query)
    {
        return await Mediator.Send(query);
    }

    /// <summary>
    /// Initiates the users withdrawal.
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [Authorize(Policy = "EmoneyWallet:Create")]
    [HttpPost("withdraw")]
    public async Task<MoneyTransferResponse> WithdrawAsync(WithdrawRequestCommand command)
    {
        return await Mediator.Send(command);
    }

    /// <summary>
    /// Returns all transactions list of the wallet.
    /// </summary>
    /// <param name="query"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize(Policy = "EmoneyWallet:ReadAll")]
    [HttpGet("{id}/transactions")]
    public async Task<ActionResult<PaginatedList<TransactionDto>>> GetWalletTransactionsAsync(
        [FromQuery] GetWalletTransactionsQuery query, Guid id)
    {
        query.WalletId = id;
        return await Mediator.Send(query);
    }

    /// <summary>
    /// Update users' wallets
    /// </summary>
    [Authorize(Policy = "EmoneyWallet:Update")]
    [HttpPut("{userId}")]
    public async Task UpdateUserWalletsAsync([FromRoute] Guid userId, UpdateUserWalletsCommand request)
    {
        request.UserId = userId;
        await Mediator.Send(request);
    }

    /// <summary>
    /// Returns the Account's wallets.
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    [Authorize(Policy = "EmoneyWallet:ReadAll")]
    [HttpGet("getAccountWallets")]
    public async Task<ActionResult<List<WalletDto>>> GetAccountWalletsAsync([FromQuery] AccountWalletsQuery query)
    {
        return await Mediator.Send(query);
    }

    [Authorize(Policy = "EmoneyWallet:Read")]
    [HttpGet("details-partner")]
    public async Task<ActionResult<WalletPartnerDto>> GetWalletDetailsPartnerAsync([FromQuery] GetWalletDetailsPartnerQuery query)
    {
        return await Mediator.Send(query);
    }

    [Authorize(Policy = "WalletBalanceDaily:ReadAll")]
    [HttpGet("wallet-balances-daily")]
    public async Task<ActionResult<WalletBalanceDailyResponse>> GetWalletBalancesDailyAsync([FromQuery] GetWalletBalancesDailyQuery query)
    {
        return await Mediator.Send(query);
    }

    /// <summary>
    /// Updates wallet balance.
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [Authorize(Policy = "WalletBalance:Update")]
    [HttpPut("update-balance")]
    public async Task<UpdateBalanceResponse> UpdateBalanceAsync(UpdateBalanceCommand command)
    {
        return await Mediator.Send(command);
    }

    /// <summary>
    /// Returns Money Transfer Types.
    /// </summary>  
    /// <returns></returns>
    [Authorize(Policy = "Parameter:ReadAll")]
    [HttpGet("MoneyTransferPaymentType")]
    public async Task<List<MoneyTransferPaymentType>> GetMoneyTransferPaymentTypeAsync()
    {
        return await Mediator.Send(new GetMoneyTransferPaymentTypeQuery { });
    }
}