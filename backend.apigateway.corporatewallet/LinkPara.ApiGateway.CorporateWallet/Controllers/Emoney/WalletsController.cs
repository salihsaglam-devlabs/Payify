using LinkPara.ApiGateway.CorporateWallet.Commons.Helpers;
using LinkPara.ApiGateway.CorporateWallet.Services.Emoney.HttpClients;
using LinkPara.ApiGateway.CorporateWallet.Services.Identity.HttpClients;
using LinkPara.ApiGateway.CorporateWallet.Services.Emoney.Models.Requests;
using LinkPara.ApiGateway.CorporateWallet.Services.Emoney.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LinkPara.ApiGateway.CorporateWallet.Services.Identity.Models.Requests;
using LinkPara.SharedModels.Exceptions;
using LinkPara.ApiGateway.CorporateWallet.Services.Identity.Models.Enums;

namespace LinkPara.ApiGateway.CorporateWallet.Controllers.Emoney;

public class WalletsController : ApiControllerBase
{
    private readonly IWalletHttpClient _walletHttpClient;
    private readonly IUserHttpClient _userHttpClient;
    private readonly IUserNameGenerator _userNameGenerator;

    public WalletsController(IWalletHttpClient walletHttpClient,
        IUserHttpClient userHttpClient,
        IUserNameGenerator userNameGenerator)
    {
        _walletHttpClient = walletHttpClient;
        _userHttpClient = userHttpClient;
        _userNameGenerator = userNameGenerator;
    }

    /// <summary>
    /// Returns the details of a wallet.
    /// </summary>
    /// <param name="request"></param>
    [Authorize(Policy = "EmoneyWallet:Read")]
    [HttpGet("details")]
    public async Task<ActionResult<WalletDto>> GetWalletDetailsAsync([FromQuery] GetWalletDetailsRequest request)
    {
        return await _walletHttpClient.GetWalletDetailsAsync(request);
    }

    /// <summary>
    /// Returns the users wallets.
    /// </summary>
    /// <returns></returns>
    [Authorize(Policy = "EmoneyWallet:Read")]
    [HttpGet("me")]
    public async Task<ActionResult<List<WalletDto>>> GetUserWalletsAsync()
    {
        return await _walletHttpClient.GetUserWalletsAsync(new GetUserWalletsFilterRequest { });
    }

    /// <summary>
    /// Creates new wallet for user.
    /// </summary>
    /// <param name="request"></param>
    [Authorize(Policy = "EmoneyWallet:Create")]
    [HttpPost("")]
    public async Task SaveWalletAsync(SaveWalletRequest request)
    {
        await _walletHttpClient.SaveWalletAsync(request);
    }

    /// <summary>
    /// Updates the FriendlyName of the users wallet.
    /// </summary>
    /// <param name="request"></param>
    [Authorize(Policy = "EmoneyWallet:Update")]
    [HttpPatch("")]
    public async Task UpdateWalletAsync(UpdateWalletRequest request)
    {
        await _walletHttpClient.UpdateWalletAsync(request);
    }

    /// <summary>
    /// Transfers money between users.
    /// </summary>
    /// <param name="request"></param>
    [Authorize(Policy = "EmoneyWallet:Create")]
    [HttpPost("transfer")]
    public async Task<MoneyTransferResponse> TransferAsync(TransferRequest request)
    {
        return await _walletHttpClient.TransferAsync(request);
    }

    /// <summary>
    /// Initiates the users withdrawal.
    /// </summary>
    /// <param name="request"></param>
    [Authorize(Policy = "EmoneyWallet:Create")]
    [HttpPost("withdraw")]
    public async Task<MoneyTransferResponse> WithdrawAsync(WithdrawRequest request)
    {
        return await _walletHttpClient.WithdrawAsync(request);
    }

    /// <summary>
    /// Returns the summary of wallet.
    /// </summary>
    /// <param name="request"></param>
    /// <exception cref="NotFoundException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    [Authorize(Policy = "EmoneyWallet:Read")]
    [HttpGet("summary")]
    public async Task<ActionResult<WalletSummaryDto>> GetWalletSummaryAsync([FromQuery] GetWalletSummaryRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.WalletNumber))
        {
            return await _walletHttpClient.GetWalletSummaryAsync(new GetWalletSummaryDetailsRequest { WalletNumber = request.WalletNumber });
        }

        if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
        {
            var userName = await _userNameGenerator.GetUserNameAsync(request.PhoneCode, request.PhoneNumber);

            var user = (await _userHttpClient.GetUsersAsync(new UserFilterRequest
            {
                UserName = userName
            }))
            .Items
            .FirstOrDefault(s => s.UserStatus == UserStatus.Active);

            if (user is null)
            {
                throw new NotFoundException(nameof(user), request.PhoneNumber);
            }

            return await _walletHttpClient.GetWalletSummaryAsync(new GetWalletSummaryDetailsRequest { UserId = user.Id });
        }

        throw new InvalidOperationException("InvalidFilterType");
    }


    /// <summary>
    /// Witdraw request preview.
    /// </summary>
    /// <param name="request"></param>
    [Authorize(Policy = "EmoneyWallet:Read")]
    [HttpGet("withdraw-preview")]
    public async Task<WithdrawPreviewResponse> WithdrawPreviewAsync([FromQuery] WithdrawPreviewRequest request)
    {
        return await _walletHttpClient.WithdrawPreviewAsync(request);
    }

    /// <summary>
    /// Transfer request preview
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "EmoneyWallet:Read")]
    [HttpGet("transfer-preview")]
    public async Task<TransferPreviewResponse> TransferPreviewAsync([FromQuery] TransferPreviewRequest request)
    {
        return await _walletHttpClient.TransferPreviewAsync(request);
    }

    /// <summary>
    /// Returns the Account's wallets.
    /// </summary>
    /// <returns></returns>
    [Authorize(Policy = "EmoneyWallet:Read")]
    [HttpGet("getAccountWallets")]
    public async Task<List<WalletDto>> GetAccountWalletsAsync([FromQuery] AccountWalletsRequest query)
    {
        return await _walletHttpClient.GetAccountWalletsAsync(query);
    }
}