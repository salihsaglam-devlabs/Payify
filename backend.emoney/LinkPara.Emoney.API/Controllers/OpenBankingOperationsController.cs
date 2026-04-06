using LinkPara.Emoney.Application.Features.OpenBankingOperations;
using LinkPara.Emoney.Application.Features.OpenBankingOperations.Queries.GetHhsList;
using LinkPara.Emoney.Application.Features.OpenBankingOperations.Commands.CreateAccountConsent;
using LinkPara.Emoney.Application.Features.OpenBankingOperations.Queries.GetHhsAccessToken;
using LinkPara.Emoney.Application.Features.OpenBankingOperations.Queries.GetAccountConsent;
using LinkPara.Emoney.Application.Features.OpenBankingOperations.Commands.DeleteAccountConsent;
using LinkPara.Emoney.Application.Features.OpenBankingOperations.Queries.GetActiveAccountConsentList;
using LinkPara.Emoney.Application.Features.OpenBankingOperations.Queries.GetConsentedAccountList;
using LinkPara.Emoney.Application.Features.OpenBankingOperations.Queries.GetConsentedAccountDetail;
using LinkPara.Emoney.Application.Features.OpenBankingOperations.Queries.GetConsentedAccountBalanceList;
using LinkPara.Emoney.Application.Features.OpenBankingOperations.Queries.GetConsentedAccountBalanceDetail;
using LinkPara.Emoney.Application.Features.OpenBankingOperations.Queries.GetConsentedAccountActivities;
using LinkPara.Emoney.Application.Features.OpenBankingOperations.Commands.CreatePaymentConsent;
using LinkPara.Emoney.Application.Features.OpenBankingOperations.Queries.GetPaymentOrderConsentDetail;
using LinkPara.Emoney.Application.Features.OpenBankingOperations.Queries.PaymentOrderDetail;
using LinkPara.Emoney.Application.Features.OpenBankingOperations.Commands.CreatePaymentOrder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using LinkPara.Emoney.Application.Features.OpenBankingOperations.Queries.GetCards;
using LinkPara.Emoney.Application.Features.OpenBankingOperations.Queries.GetCardDetail;
using LinkPara.Emoney.Application.Features.OpenBankingOperations.Commands.CreateFuturePaymentOrderConsent;
using LinkPara.Emoney.Application.Features.OpenBankingOperations.Commands.TriggerFuturePaymentOrder;
using LinkPara.Emoney.Application.Features.OpenBankingOperations.Commands.CancelFuturePaymentOrder;
using LinkPara.Emoney.Application.Features.OpenBankingOperations.Commands.CreateStandingPaymentOrderConsent;
using LinkPara.Emoney.Application.Features.OpenBankingOperations.Queries.GetCardTransactions;

namespace LinkPara.Emoney.API.Controllers
{
    public class OpenBankingOperationsController : ApiControllerBase
    {

        /// <summary>
        /// This method is used to get customer's hhs list.
        /// </summary>
        /// <returns>it returns hhs list</returns>
        [Authorize(Policy = "OpenBankingOperation:ReadAll")]
        [HttpGet("hhs-list")]
        public async Task<ActionResult<HhsResultDto>> GetHhsListAsync()
        {
            return await Mediator.Send(new GetHhsListQuery());
        }

        /// <summary>
        /// This method is used to create account consent.
        /// </summary>
        /// <param name="command"></param>
        /// <returns>it returns consent detail information.</returns>
        [Authorize(Policy = "OpenBankingOperation:Create")]
        [HttpPost("create-account-consent")]
        public async Task<ActionResult<AccountConsentDetailResultDto>> CreateAccountConsentAsync([FromBody] CreateAccountConsentCommand command)
        {
            return await Mediator.Send(command);
        }

        /// <summary>
        /// This method is used to get access token for hhs application.
        /// </summary>
        /// <param name="query"></param>
        /// <returns>it returns group id for hhs</returns>
        [Authorize(Policy = "OpenBankingOperation:Read")]
        [HttpGet("hhs-token")]
        public async Task<ActionResult<YosServiceResultDto>> GetHhsAccessTokenAsync([FromQuery] GetHhsAccessTokenQuery query)
        {
            return await Mediator.Send(query);
        }

        /// <summary>
        /// This method is used to get detail information of an account consent.
        /// </summary>
        /// <param name="consentId"></param>
        /// <returns>it returns consent detail information.</returns>
        [Authorize(Policy = "OpenBankingOperation:Read")]
        [HttpGet("account-consent-detail")]
        public async Task<ActionResult<AccountConsentDetailResultDto>> GetAccountConsentDetailAsync(string consentId)
        {
            return await Mediator.Send(new GetAccountConsentQuery { ConsentId = consentId});
        }

        /// <summary>
        /// This method is used to delete account consent.
        /// </summary>
        /// <param name="consentId"></param>
        /// <returns>it returns success when the consent deleted.</returns>
        [Authorize(Policy = "OpenBankingOperation:Delete")]
        [HttpDelete("delete-account-consent")]
        public async Task<ActionResult<YosServiceResultDto>> DeleteAccountConsentAsync(string consentId)
        {
            return await Mediator.Send(new DeleteAccountConsentCommand { ConsentId = consentId });
        }

        /// <summary>
        /// This method is used to get active account consent list.
        /// </summary>
        /// <param name="query"></param>
        /// <returns>it returns active account consents of customer</returns>
        [Authorize(Policy = "OpenBankingOperation:ReadAll")]
        [HttpGet("active-account-consent")]
        public async Task<ActionResult<ActiveAccountConsentResultDto>> GetActiveAccountConsentListAsync([FromQuery] GetActiveAccountConsentListQuery query)
        {
            return await Mediator.Send(query);
        }

        /// <summary>
        /// This method is used to get consented account list.
        /// </summary>
        /// <param name="query"></param>
        /// <returns>it returns consented account list</returns>
        [Authorize(Policy = "OpenBankingOperation:ReadAll")]
        [HttpGet("consented-accounts")]
        public async Task<ActionResult<ConsentedAccountsResultDto>> GetConsentedAccountListAsync([FromQuery] GetConsentedAccountListQuery query)
        {
            return await Mediator.Send(query);
        }

        /// <summary>
        /// This method is used to get detail information of a consented account.
        /// </summary>
        /// <param name="query"></param>
        /// <returns>it returns detail information of a consented account</returns>
        [Authorize(Policy = "OpenBankingOperation:Read")]
        [HttpGet("consented-account-detail")]
        public async Task<ActionResult<ConsentedAccountDetailResultDto>> GetConsentedAccountDetailAsync([FromQuery] GetConsentedAccountDetailQuery query)
        {
            return await Mediator.Send(query);
        }

        /// <summary>
        /// This method is used to get balance list of consented accounts.
        /// </summary>
        /// <param name="query"></param>
        /// <returns>it returns balance list.</returns>
        [Authorize(Policy = "OpenBankingOperation:ReadAll")]
        [HttpGet("account-balances")]
        public async Task<ActionResult<ConsentedAccountBalancesResultDto>> GetConsentedAccountBalanceListAsync([FromQuery] GetConsentedAccountBalanceListQuery query)
        {
            return await Mediator.Send(query);
        }

        /// <summary>
        /// This method is used to get balance information of a consented account.
        /// </summary>
        /// <param name="query"></param>
        /// <returns>it returns balance information of consented account</returns>
        [Authorize(Policy = "OpenBankingOperation:Read")]
        [HttpGet("account-balance-detail")]
        public async Task<ActionResult<ConsentedAccountBalanceDetailResultDto>> GetConsentedAccountBalanceDetailAsync([FromQuery] GetConsentedAccountBalanceDetailQuery query)
        {
            return await Mediator.Send(query);
        }

        /// <summary>
        /// This method is used to get account activities of a consented account.
        /// </summary>
        /// <param name="query"></param>
        /// <returns>it returns account activities</returns>
        [Authorize(Policy = "OpenBankingOperation:ReadAll")]
        [HttpGet("account-activities")]
        public async Task<ActionResult<ConsentedAccountActivitiesResultDto>> GetConsentedAccountActivitiesAsync([FromQuery] GetConsentedAccountActivitiesQuery query)
        {
            return await Mediator.Send(query);
        }

        /// <summary>
        /// This method is used to create payment consent.
        /// </summary>
        /// <param name="command"></param>
        /// <returns>it returns payment consent detail information.</returns>
        [Authorize(Policy = "OpenBankingOperation:Create")]
        [HttpPost("create-payment-consent")]
        public async Task<ActionResult<PaymentOrderConsentDetailDto>> CreatePaymentConsentAsync([FromBody] CreatePaymentConsentCommand command)
        {
            return await Mediator.Send(command);
        }

        /// <summary>
        /// This method is used to get detail information of a payment order consent.
        /// </summary>
        /// <param name="query"></param>
        /// <returns>it returns payment consent detail information.</returns>
        [Authorize(Policy = "OpenBankingOperation:Read")]
        [HttpGet("payment-order-consent")]
        public async Task<ActionResult<PaymentOrderConsentDetailDto>> GetPaymentOrderConsentDetailAsync([FromQuery] GetPaymentOrderConsentDetailQuery query)
        {
            return await Mediator.Send(query);
        }

        /// <summary>
        /// This method is used to create payment order.
        /// </summary>
        /// <param name="command"></param>
        /// <returns>it returns payment order detail information.</returns>
        [Authorize(Policy = "OpenBankingOperation:Create")]
        [HttpPost("create-payment-order")]
        public async Task<ActionResult<PaymentOrderDetailResultDto>> CreatePaymentOrderAsync([FromBody] CreatePaymentOrderYosCommand command)
        {
            return await Mediator.Send(command);
        }

        /// <summary>
        /// This method is used to get detail information of a payment order.
        /// </summary>
        /// <param name="query"></param>
        /// <returns>it returns payment order detail information.</returns>
        [Authorize(Policy = "OpenBankingOperation:Read")]
        [HttpGet("payment-order-detail")]
        public async Task<ActionResult<PaymentOrderDetailResultDto>> PaymentOrderDetailAsync([FromQuery] PaymentOrderDetailQuery query)
        {
            return await Mediator.Send(query);
        }

        /// <summary>
        /// This method is used to get card list
        /// </summary>
        /// <param name="query"></param>
        /// <returns>it returns consented card list .</returns>
        [Authorize(Policy = "OpenBankingOperation:Read")]
        [HttpGet("cards")]
        public async Task<ActionResult<CardsResultDto>> GetCardsAsync([FromQuery] GetCardsQuery query)
        {
            return await Mediator.Send(query);
        }

        /// <summary>
        /// This method is used to get card detalis 
        /// </summary>
        /// <param name="query"></param>
        /// <returns>it returns consented card detail list .</returns>
        [Authorize(Policy = "OpenBankingOperation:Read")]
        [HttpGet("card-detail")]
        public async Task<ActionResult<CardDetailResultDto>> GetCardDetailAsync([FromQuery] GetCardDetailQuery query)
        {
            return await Mediator.Send(query);
        }

        /// <summary>
        /// This method is used to get card transactions 
        /// </summary>
        /// <param name="query"></param>
        /// <returns>it returns card transactions.</returns>
        [Authorize(Policy = "OpenBankingOperation:Read")]
        [HttpGet("card-transactions")]
        public async Task<ActionResult<CardTransactionsResultDto>> GetCardTransactionAsync([FromQuery] GetCardTransactionsQuery query)
        {
            return await Mediator.Send(query);
        }

        /// <summary>
        /// This method is used to create future payment order consent
        /// </summary>
        /// <param name="command"></param>
        /// <returns>it returns FuturePaymentOrderConsentResultDto .</returns>
        [Authorize(Policy = "OpenBankingOperation:Create")]
        [HttpPost("create-future-payment-order-consent")]
        public async Task<ActionResult<FuturePaymentOrderConsentResultDto>> CreateFuturePaymentOrderConsentAsync([FromBody] CreateFuturePaymentOrderConsentCommand command)
        {
            return await Mediator.Send(command);
        }

        /// <summary>
        /// This method is used to list future payment orders
        /// </summary>
        /// <param name="command"></param>
        /// <returns>it returns GetFuturePaymentOrderListResultDto.</returns>
        [Authorize(Policy = "OpenBankingOperation:Read")]
        [HttpPost("list-future-payment-order")]
        public async Task<ActionResult<GetFuturePaymentOrderListResultDto>> GetFuturePaymentOrderListAsync([FromBody] GetFuturePaymentOrderListQuery query)
        {
            return await Mediator.Send(query);
        }

        /// <summary>
        /// This method is used to trigger future payment order
        /// </summary>
        /// <param name="command"></param>
        /// <returns>it returns TriggerFuturePaymentOrderResultDto.</returns>
        [Authorize(Policy = "OpenBankingOperation:Read")]
        [HttpPost("trigger-future-payment-order")]
        public async Task<ActionResult<TriggerFuturePaymentOrderResultDto>> TriggerFuturePaymentOrderAsync([FromBody] TriggerFuturePaymentOrderCommand command)
        {
            return await Mediator.Send(command);
        }

        /// <summary>
        /// This method is used to trigger future payment order
        /// </summary>
        /// <param name="command"></param>
        /// <returns>it returns CancelFuturePaymentOrderAsync .</returns>
        [Authorize(Policy = "OpenBankingOperation:Create")]
        [HttpPost("cancel-future-payment-order")]
        public async Task<ActionResult<CancelFuturePaymentOrderResultDto>> CancelFuturePaymentOrderAsync([FromBody] CancelFuturePaymentOrderCommand command)
        {
            return await Mediator.Send(command);
        }

        /// <summary>
        /// This method is used to create recurring payment order consent
        /// </summary>
        /// <param name="command"></param>
        /// <returns>it returns StandingPaymentOrderConsentResultDto.</returns>
        [Authorize(Policy = "OpenBankingOperation:Create")]
        [HttpPost("create-standing-payment-order-consent")]
        public async Task<ActionResult<StandingPaymentOrderConsentResultDto>> CreateStandingPaymentOrderConsentAsync([FromBody] CreateStandingPaymentOrderConsentCommand command)
        {
            return await Mediator.Send(command);
        }


    }
}