using LinkPara.ApiGateway.OpenBanking.Services.Emoney.HttpClients;
using LinkPara.ApiGateway.OpenBanking.Services.Emoney.Requests;
using LinkPara.ApiGateway.OpenBanking.Services.Emoney.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.OpenBanking.Controllers;

[Authorize(Policy = "HHS")]
public class AccountServiceProvidersController : ApiControllerBase
{
    private readonly IAccountServiceProviderHttpClient _httpClient;

    public AccountServiceProvidersController(IAccountServiceProviderHttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// customer confirmation service
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("customer-confirmation")]
    public async Task<BaseServiceResponse<CustomerConfirmationResponse>> CustomerConfirmationAsync([FromBody] CustomerConfirmationRequest request)
    {
        return await _httpClient.CustomerConfirmationAsync(request);
    }

    /// <summary>
    /// This method used to get customer account transactions.
    /// </summary>
    /// <param name="request"></param>
    /// <returns>it returns account transaction list</returns>
    [HttpGet("account-transaction")]
    public async Task<ActionResult<AccountTransactionsDto>> GetAccountTransactionsAsync([FromQuery] GetAccountTransactionsRequest request)
    {
        return await _httpClient.GetAccountTransactionsAsync(request);
    }

    /// <summary>
    /// This method is used to inquire about accounts with balance changes.
    /// </summary>
    /// <returns>It is the object that will contain the list of accounts that the customer can transact with.</returns>
    [HttpGet("changed-balances")]
    public async Task<ActionResult<List<ChangedBalanceDto>>> GetChangedBalanceAsync()
    {
        return await _httpClient.GetChangedBalanceAsync();
    }

    /// <summary>
    /// This method is used to notify the client when a request to create a consent of the discrete GKD type arrives.
    /// </summary>
    /// <param name="request"></param>
    /// <returns>it returns true when the notification is sent successfully.</returns>
    [HttpPost("gkd-notification")]
    public async Task<ActionResult<SendNotificationResultDto>> SendGkdNotificationAsync([FromBody] SendGkdNotificationRequest request)
    {
        return await _httpClient.SendGkdNotificationAsync(request);
    }

    /// <summary>
    /// This method is used to send sms otp to the customer.
    /// </summary>
    /// <param name="request"></param>
    /// <returns>it returns true when the otp is sent successfully.</returns>
    [HttpPost("otp-message")]
    public async Task<ActionResult<SendOtpMessageResultDto>> SendOtpMessageAsync([FromBody] SendOtpMessageRequest request)
    {
        return await _httpClient.SendOtpMessageAsync(request);
    }

    /// <summary>
    /// This method is used to perform money transfer transactions.
    /// </summary>
    /// <param name="request"></param>
    /// <returns>it returns true when the payment order is created successfully.</returns>
    [HttpPost("create-payment-order")]
    public async Task<BaseServiceResponse<PaymentContractDto>> CreatePaymentOrderAsync([FromBody] CreatePaymentOrderRequest request)
    {
        return await _httpClient.CreatePaymentOrderAsync(request);
    }

    /// <summary>
    /// This method is used to inquire the latest status of a payment order in which a money transfer has taken place.
    /// </summary>
    /// <param name="request"></param>
    /// <returns>it returns detail information about the payment order.</returns>
    [HttpPost("payment-order-inquiry")]
    public async Task<BaseServiceResponse<PaymentContractDto>> PaymentOrderInquiryAsync([FromBody] PaymentOrderInquiryRequest request)
    {
        return await _httpClient.PaymentOrderInquiryAsync(request);
    }

    /// <summary>
    /// This method used to get customer wallet list for approval screen.
    /// </summary>
    /// <param name="request"></param>
    /// <returns>it returns wallet list</returns>
    [HttpGet("approval-screen-wallet-list")]
    public async Task<ActionResult<GetApprovalScreenWalletListResponse>> GetApprovalScreenWalletsAsync([FromQuery] GetApprovalScreenWalletListRequest request)
    {
        return await _httpClient.GetApprovalScreenWalletsAsync(request);
    }

    /// <summary>
    /// This method used to get customer wallet list.
    /// </summary>
    /// <returns>it returns wallet list</returns>
    [HttpGet("wallet-list")]
    public async Task<ActionResult<GetWalletListResponse>> GetWalletsAsync([FromQuery] GetWalletInfoRequest request)
    {
        return await _httpClient.GetWalletsAsync(request);
    }


    /// <summary>
    /// This method used to get customer wallet balance list.
    /// </summary>
    /// <returns>it returns wallet list</returns>
    [HttpGet("wallet-balance-list")]
    public async Task<ActionResult<GetWalletBalanceListResponse>> GetWalletBalancesAsync([FromQuery] GetWalletInfoRequest request)
    {
        return await _httpClient.GetWalletBalancesAsync(request);
    }
}
