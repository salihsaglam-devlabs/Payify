using LinkPara.Emoney.Application.Features.AccountServiceProviders;
using LinkPara.Emoney.Application.Features.AccountServiceProviders.Commands;
using LinkPara.Emoney.Application.Features.AccountServiceProviders.Commands.CreatePaymentOrder;
using LinkPara.Emoney.Application.Features.AccountServiceProviders.Commands.SendGkdNotification;
using LinkPara.Emoney.Application.Features.AccountServiceProviders.Commands.SendOtpMessage;
using LinkPara.Emoney.Application.Features.AccountServiceProviders.Queries.GetAccountTransactions;
using LinkPara.Emoney.Application.Features.AccountServiceProviders.Queries.GetApprovalScreenWalletList;
using LinkPara.Emoney.Application.Features.AccountServiceProviders.Queries.GetChangedBalance;
using LinkPara.Emoney.Application.Features.AccountServiceProviders.Queries.GetWalletBalanceList;
using LinkPara.Emoney.Application.Features.AccountServiceProviders.Queries.GetWalletList;
using LinkPara.Emoney.Application.Features.AccountServiceProviders.Queries.PaymentOrderInquiry;
using LinkPara.Emoney.Application.Features.AccountServiceProviders.Queries.GetUserIdentityInfo;
using LinkPara.Emoney.Application.Features.AccountServiceProviders.Queries.GetUserAccountList;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.Emoney.API.Controllers;

public class AccountServiceProvidersController : ApiControllerBase
{
    /// <summary>
    /// This method used to customer confirmation. 
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [HttpPost("customer-confirmation")]
    public async Task<CustomerConfirmationDto> CustomerConfirmationAsync([FromBody] CustomerConfirmationCommand command)
    {
        return await Mediator.Send(command);
    }

    /// <summary>
    /// This method used to get customer account transactions.
    /// </summary>
    /// <param name="AccountId"></param>
    /// <param name="request"></param>
    /// <returns>it returns account transaction list</returns>
    [HttpGet("account-transaction/{AccountId}")]
    public async Task<ActionResult<AccountTransactionsDto>> GetAccountTransactionsAsync([FromRoute] Guid AccountId,[FromQuery] GetAccountTransactionsQuery request)
    {
        request.AccountId = AccountId;
        return await Mediator.Send(request);
    }

    /// <summary>
    /// This method is used to inquire about accounts with balance changes.
    /// </summary>
    /// <returns>It is the object that will contain the list of accounts that the customer can transact with.</returns>
    [HttpGet("changed-balances")]
    public async Task<ActionResult<List<ChangedBalanceDto>>> GetChangedBalanceAsync()
    {
        return await Mediator.Send(new GetChangedBalanceQuery());
    }

    /// <summary>
    /// This method is used to notify the client when a request to create a consent of the discrete GKD type arrives.
    /// </summary>
    /// <param name="AccountId"></param>
    /// <param name="request"></param>
    /// <returns>it returns true when the notification is sent successfully.</returns>
    [HttpPost("gkd-notification/{AccountId}")]
    public async Task<ActionResult<SendNotificationResultDto>> SendGkdNotificationAsync([FromRoute] Guid AccountId, [FromBody] SendGkdNotificationCommand request)
    {
        request.AccountId = AccountId;
        return await Mediator.Send(request);
    }

    /// <summary>
    /// This method is used to send sms otp to the customer.
    /// </summary>
    /// <param name="AccountId"></param>
    /// <param name="request"></param>
    /// <returns>it returns true when the otp is sent successfully.</returns>
    [HttpPost("otp-message/{AccountId}")]
    public async Task<ActionResult<SendOtpMessageResultDto>> SendOtpMessageAsync([FromRoute] Guid AccountId, [FromBody] SendOtpMessageCommand request)
    {
        request.AccountId = AccountId;
        return await Mediator.Send(request);
    }

    /// <summary>
    /// This method is used to perform money transfer transactions.
    /// </summary>
    /// <param name="request"></param>
    /// <returns>it returns true when the payment order is created successfully.</returns>
    [HttpPost("create-payment-order")]
    public async Task<ActionResult<PaymentContractDto>> CreatePaymentOrderAsync([FromBody] CreatePaymentOrderCommand request)
    {
        return await Mediator.Send(request);
    }

    /// <summary>
    /// This method is used to inquire the latest status of a payment order in which a money transfer has taken place.
    /// </summary>
    /// <param name="request"></param>
    /// <returns>it returns detail information about the payment order.</returns>
    [HttpPost("payment-order-inquiry")]
    public async Task<ActionResult<PaymentContractDto>> PaymentOrderInquiryAsync([FromQuery] PaymentOrderInquiryQuery request)
    {
        return await Mediator.Send(request);
    }

    /// <summary>
    /// This method used to get customer wallet list.
    /// </summary>
    /// <param name="query"></param>
    /// <returns>it returns wallet list</returns>
    [HttpGet("approval-screen-wallet-list")]
    public async Task<List<ApprovalScreenWalletDto>> GetWalletsAsync([FromQuery] GetApprovalScreenWalletListQuery query)
    {
        return await Mediator.Send(query);
    }

    /// <summary>
    /// This method used to get customer wallet list.
    /// </summary>
    /// <returns>it returns wallet list</returns>
    [HttpGet("wallet-list/{accountId}")]
    public async Task<List<CustomerWalletDto>> GetWalletsAsync([FromRoute] Guid accountId, [FromQuery] GetWalletListQuery query)
    {
        query.AccountId = accountId;
        return await Mediator.Send(query);
    }


    /// <summary>
    /// This method used to get customer wallet balance list.
    /// </summary>
    /// <returns>it returns wallet list</returns>
    [HttpGet("wallet-balance-list/{accountId}")]
    public async Task<List<CustomerWalletBalanceDto>> GetWalletBalancesAsync([FromRoute] Guid accountId, [FromQuery] GetWalletBalanceListQuery query)
    {
        query.AccountId = accountId;
        return await Mediator.Send(query);
    }

    /// <summary>
    /// This method used to get customer wallet list for approval screen.
    /// </summary>
    /// <param name="query"></param>
    /// <returns>it returns wallet list</returns>
    [HttpGet("user-identity-info")]
    public async Task<ActionResult<IdentityInfoDto>> GetUserIdentityInfoAsync([FromQuery] GetUserIdentityInfoQuery query)
    {
        return await Mediator.Send(query);
    }

    /// <summary>
    /// This method used to get user account list information.
    /// </summary>
    /// <param name="query"></param>
    /// <returns>it returns user account list</returns>
    [HttpGet("user-accounts")]
    public async Task<ActionResult<List<UserAccountResultDto>>> GetUserAccountListAsync([FromQuery] GetUserAccountListQuery query)
    {
        return await Mediator.Send(query);
    }
}
