using LinkPara.ApiGateway.Boa.Commons.Helpers;
using LinkPara.ApiGateway.Boa.Filters.CustomerContext;
using LinkPara.ApiGateway.Boa.Services.Emoney.HttpClients;
using LinkPara.ApiGateway.Boa.Services.Emoney.Models.Requests;
using LinkPara.ApiGateway.Boa.Services.Emoney.Models.Responses;
using LinkPara.ApiGateway.Boa.Services.Identity.HttpClients;
using LinkPara.ApiGateway.Boa.Services.Identity.Models.Requests;
using LinkPara.SharedModels.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.Boa.Controllers.Emoney;

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
    [HttpGet("details")]
    [CustomerContextRequired]
    public async Task<ActionResult<WalletDto>> GetWalletDetailsAsync([FromQuery] GetWalletDetailsRequest request)
    {
        return await _walletHttpClient.GetWalletDetailsAsync(request);
    }

    /// <summary>
    /// Returns the users wallets.
    /// </summary>
    /// <returns></returns>
    [HttpGet("me")]
    [CustomerContextRequired]
    public async Task<ActionResult<List<WalletDto>>> GetUserWalletsAsync()
    {
        return await _walletHttpClient.GetUserWalletsAsync(new GetUserWalletsFilterRequest { });
    }

    /// <summary>
    /// Updates the FriendlyName of the users wallet.
    /// </summary>
    /// <param name="request"></param>
    [HttpPatch("")]
    [CustomerContextRequired]
    public async Task UpdateWalletAsync(UpdateWalletRequest request)
    {
        await _walletHttpClient.UpdateWalletAsync(request);
    }

    /// <summary>
    /// Transfers money between users.
    /// </summary>
    /// <param name="request"></param>
    [HttpPost("transfer")]
    [CustomerContextRequired]
    public async Task<MoneyTransferResponse> TransferAsync(TransferRequest request)
    {
        return await _walletHttpClient.TransferAsync(request);
    }

    /// <summary>
    /// Initiates the users withdrawal.
    /// </summary>
    /// <param name="request"></param>
    [HttpPost("withdraw")]
    [CustomerContextRequired]
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
    [HttpGet("summary")]
    [CustomerContextRequired]
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
            .FirstOrDefault(s => s.UserStatus == Services.Identity.Models.Enums.UserStatus.Active);

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
    [HttpGet("withdraw-preview")]
    [CustomerContextRequired]
    public async Task<WithdrawPreviewResponse> WithdrawPreviewAsync([FromQuery] WithdrawPreviewRequest request)
    {
        return await _walletHttpClient.WithdrawPreviewAsync(request);
    }

    /// <summary>
    /// Transfer request preview
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpGet("transfer-preview")]
    [CustomerContextRequired]
    public async Task<TransferPreviewResponse> TransferPreviewAsync([FromQuery] TransferPreviewRequest request)
    {
        return await _walletHttpClient.TransferPreviewAsync(request);
    }

    /// <summary>
    /// Returns the Account's wallets.
    /// </summary>
    /// <returns></returns>
    [HttpGet("getAccountWallets")]
    [CustomerContextRequired]
    public async Task<List<WalletDto>> GetAccountWalletsAsync([FromQuery] AccountWalletsRequest query)
    {
        return await _walletHttpClient.GetAccountWalletsAsync(query);
    }

    /// <summary>
    /// Updates users wallet balance.
    /// </summary>
    /// <returns></returns>    
    [HttpPut("update-balance")]
    public async Task<UpdateBalanceResponse> UpdateBalanceAsync(UpdateBalanceRequest request)
    {
        return await _walletHttpClient.UpdateBalanceAsync(request);
    }

    /// <summary>
    /// Returns the Money Transfer Types.
    /// </summary>
    /// <returns></returns>
    [HttpGet("MoneyTransferPaymentType")]
    [CustomerContextRequired]
    public async Task<List<MoneyTransferPaymentType>> GetMoneyTransferPaymentTypeAsync()
    {
        return await _walletHttpClient.GetMoneyTransferPaymentTypeAsync();
    }
}
