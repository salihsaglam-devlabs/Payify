using LinkPara.ApiGateway.Commons.Helpers;
using LinkPara.ApiGateway.Services.Emoney;
using LinkPara.ApiGateway.Services.Emoney.HttpClients;
using LinkPara.ApiGateway.Services.Emoney.Models.Requests;
using LinkPara.ApiGateway.Services.Emoney.Models.Responses;
using LinkPara.ApiGateway.Services.Identity.HttpClients;
using LinkPara.ApiGateway.Services.Identity.Models.Enums;
using LinkPara.ApiGateway.Services.Identity.Models.Requests;
using LinkPara.SharedModels.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.Controllers.Emoney;

public class WalletsController : ApiControllerBase
{
    private readonly IWalletHttpClient _walletHttpClient;
    private readonly IUserHttpClient _userHttpClient;
    private readonly IUserNameGenerator _userNameGenerator;
    private readonly IAuthorizationService _authorizationService;
    private readonly IPaymentOtpRequirementService _paymentOtpRequirementService;

    public WalletsController(IWalletHttpClient walletHttpClient,
        IUserHttpClient userHttpClient,
        IUserNameGenerator userNameGenerator,
        IAuthorizationService authorizationService,
        IPaymentOtpRequirementService paymentOtpRequirementService)
    {
        _walletHttpClient = walletHttpClient;
        _userHttpClient = userHttpClient;
        _userNameGenerator = userNameGenerator;
        _authorizationService = authorizationService;
        _paymentOtpRequirementService = paymentOtpRequirementService;
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
    /// <param name="otpCode"></param>
    /// <param name="otpAuthorizationId"></param>
    /// <param name="otpTimestamp"></param>
    [Authorize(Policy = "EmoneyWallet:Update")]
    [HttpPost("transfer")]
    public async Task<MoneyTransferResponse> TransferAsync(TransferRequest request, 
        [FromHeader(Name = "otp-code")] string otpCode,
        [FromHeader(Name = "otp-authorization-id")] string otpAuthorizationId,
        [FromHeader(Name = "otp-timestamp")] string otpTimestamp)
    {
        var otpRequirement = await _paymentOtpRequirementService.IsRequireOtp(request.Amount);

        if (otpRequirement)
        {
            var result = await _authorizationService.AuthorizeAsync(User, null, "RequireOtp");

            if (!result.Succeeded)
            {
                throw new InvalidOtpException();
            }
        }

        return await _walletHttpClient.TransferAsync(request);
    }

    /// <summary>
    /// Initiates the users withdrawal.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="otpCode"></param>
    /// <param name="otpAuthorizationId"></param>
    /// <param name="otpTimestamp"></param>
    [Authorize(Policy = "EmoneyWallet:Update")]
    [HttpPost("withdraw")]
    public async Task<MoneyTransferResponse> WithdrawAsync(WithdrawRequest request,
        [FromHeader(Name = "otp-code")] string otpCode,
        [FromHeader(Name = "otp-authorization-id")] string otpAuthorizationId,
        [FromHeader(Name = "otp-timestamp")] string otpTimestamp)
    {
        var otpRequirement = await _paymentOtpRequirementService.IsRequireOtp(request.Amount);

        if (otpRequirement)
        {
            var result = await _authorizationService.AuthorizeAsync(User, null, "RequireOtp");

            if (!result.Succeeded)
            {
                throw new InvalidOtpException();
            }
        }

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


    /// <summary>
    /// This method used to money transfer for logged in user.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("transfer-for-logged-in-user")]
    [Authorize(Policy = "PayWithWallet:Create")]
    public async Task<ActionResult<PayWithWalletResponse>> TransferForLoggedInUserAsync([FromBody] TransferForLoggedInUserRequest request)
    {
        return await _walletHttpClient.TransferForLoggedInUserAsync(request);

    }

    /// <summary>
    /// Returns Money Transfer Types.
    /// </summary>
    /// <returns></returns>
    [Authorize(Policy = "Parameter:ReadAll")]
    [HttpGet("MoneyTransferPaymentType")]
    public async Task<List<MoneyTransferPaymentType>> GetMoneyTransferPaymentTypeAsync()
    {
        return await _walletHttpClient.GetMoneyTransferPaymentTypeAsync();
    }

    /// <summary>
    /// Returns Otp Requirements.
    /// </summary>
    /// <returns></returns>
    [Authorize(Policy = "EmoneyWallet:Read")]
    [HttpGet("is-require-otp")]
    public async Task<bool> GetOtpRequirementsAsync(decimal amount)
    {
        return await _paymentOtpRequirementService.IsRequireOtp(amount);
    }
}