using LinkPara.ApiGateway.OpenBanking.Services.Emoney.Responses;
using LinkPara.ApiGateway.Services.Emoney.HttpClients;
using LinkPara.ApiGateway.Services.Emoney.Models.Requests;
using LinkPara.ApiGateway.Services.Emoney.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;

namespace LinkPara.ApiGateway.Controllers.Emoney
{
    public class OpenBankingOperationsController : ApiControllerBase
    {
        private readonly IOpenBankingOperationHttpClient _openBankingOperationHttpClient;

        public OpenBankingOperationsController(IOpenBankingOperationHttpClient openBankingOperationHttpClient)
        {
            _openBankingOperationHttpClient = openBankingOperationHttpClient;
        }

        /// <summary>
        /// This method is used to get customer's hhs list.
        /// </summary>
        /// <returns>it returns hhs list</returns>
        [Authorize(Policy = "OpenBankingOperation:ReadAll")]
        [HttpGet("hhs-list")]
        public async Task<ActionResult<HhsResultDto>> GetHhsListAsync()
        {
            return await _openBankingOperationHttpClient.GetHhsListAsync();
        }

        /// <summary>
        /// This method is used to create account consent.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>it returns consent detail information.</returns>
        [Authorize(Policy = "OpenBankingOperation:Create")]
        [HttpPost("create-account-consent")]
        public async Task<ActionResult<AccountConsentDetailResultDto>> CreateAccountConsentAsync([FromBody] CreateAccountConsentRequest request)
        {
            return await _openBankingOperationHttpClient.CreateAccountConsentAsync(request);
        }

        /// <summary>
        /// This method is used to get access token for hhs application.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>it returns group id for hhs</returns>
        [Authorize(Policy = "OpenBankingOperation:Read")]
        [HttpGet("hhs-token")]
        public async Task<ActionResult<YosServiceResultDto>> GetHhsAccessTokenAsync([FromQuery] GetHhsAccessTokenRequest request)
        {
            return await _openBankingOperationHttpClient.GetHhsAccessTokenAsync(request);
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
            return await _openBankingOperationHttpClient.GetAccountConsentDetailAsync(consentId);
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
            return await _openBankingOperationHttpClient.DeleteAccountConsentAsync(consentId);
        }

        /// <summary>
        /// This method is used to get active account consent list.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>it returns active account consents of customer</returns>
        [Authorize(Policy = "OpenBankingOperation:ReadAll")]
        [HttpGet("active-account-consent")]
        public async Task<ActionResult<ActiveAccountConsentResultDto>> GetActiveAccountConsentListAsync([FromQuery] GetActiveAccountConsentListRequest request)
        {
            return await _openBankingOperationHttpClient.GetActiveAccountConsentListAsync(request);
        }

        /// <summary>
        /// This method is used to get consented account list.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>it returns consented account list</returns>
        [Authorize(Policy = "OpenBankingOperation:ReadAll")]
        [HttpGet("consented-accounts")]
        public async Task<ActionResult<ConsentedAccountsResultDto>> GetConsentedAccountListAsync([FromQuery] GetConsentedAccountListRequest request)
        {
            return await _openBankingOperationHttpClient.GetConsentedAccountListAsync(request);
        }

        /// <summary>
        /// This method is used to get detail information of a consented account.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>it returns detail information of a consented account</returns>
        [Authorize(Policy = "OpenBankingOperation:Read")]
        [HttpGet("consented-account-detail")]
        public async Task<ActionResult<ConsentedAccountDetailResultDto>> GetConsentedAccountDetailAsync([FromQuery] GetConsentedAccountDetailRequest request)
        {
            return await _openBankingOperationHttpClient.GetConsentedAccountDetailAsync(request);
        }

        /// <summary>
        /// This method is used to get balance list of consented accounts.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>it returns balance list.</returns>
        [Authorize(Policy = "OpenBankingOperation:ReadAll")]
        [HttpGet("account-balances")]
        public async Task<ActionResult<ConsentedAccountBalancesResultDto>> GetConsentedAccountBalanceListAsync([FromQuery] GetConsentedAccountBalanceListRequest request)
        {
            return await _openBankingOperationHttpClient.GetConsentedAccountBalanceListAsync(request);
        }

        /// <summary>
        /// This method is used to get balance information of a consented account.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>it returns balance information of consented account</returns>
        [Authorize(Policy = "OpenBankingOperation:Read")]
        [HttpGet("account-balance-detail")]
        public async Task<ActionResult<ConsentedAccountBalanceDetailResultDto>> GetConsentedAccountBalanceDetailAsync([FromQuery] GetConsentedAccountBalanceDetailRequest request)
        {
            return await _openBankingOperationHttpClient.GetConsentedAccountBalanceDetailAsync(request);
        }

        /// <summary>
        /// This method is used to get account activities of a consented account.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>it returns account activities</returns>
        [Authorize(Policy = "OpenBankingOperation:ReadAll")]
        [HttpGet("account-activities")]
        public async Task<ActionResult<ConsentedAccountActivitiesResultDto>> GetConsentedAccountActivitiesAsync([FromQuery] GetConsentedAccountActivitiesRequest request)
        {
            return await _openBankingOperationHttpClient.GetConsentedAccountActivitiesAsync(request);
        }

        /// <summary>
        /// This method is used to create payment consent.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>it returns payment consent detail information.</returns>
        [Authorize(Policy = "OpenBankingOperation:Create")]
        [HttpPost("create-payment-consent")]
        public async Task<ActionResult<PaymentOrderConsentDetailDto>> CreatePaymentConsentAsync([FromBody] CreatePaymentConsentRequest request)
        {
            return await _openBankingOperationHttpClient.CreatePaymentConsentAsync(request);
        }

        /// <summary>
        /// This method is used to get detail information of a payment order consent.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>it returns payment consent detail information.</returns>
        [Authorize(Policy = "OpenBankingOperation:Read")]
        [HttpGet("payment-order-consent")]
        public async Task<ActionResult<PaymentOrderConsentDetailDto>> GetPaymentOrderConsentDetailAsync([FromQuery] GetPaymentOrderConsentDetailRequest request)
        {
            return await _openBankingOperationHttpClient.GetPaymentOrderConsentDetailAsync(request);
        }

        /// <summary>
        /// This method is used to create payment order.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>it returns payment order detail information.</returns>
        [Authorize(Policy = "OpenBankingOperation:Create")]
        [HttpPost("create-payment-order")]
        public async Task<ActionResult<PaymentOrderDetailResultDto>> CreatePaymentOrderAsync([FromBody] CreatePaymentOrderRequest request)
        {
            return await _openBankingOperationHttpClient.CreatePaymentOrderAsync(request);
        }

        /// <summary>
        /// This method is used to get detail information of a payment order.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>it returns payment order detail information.</returns>
        [Authorize(Policy = "OpenBankingOperation:Read")]
        [HttpGet("payment-order-detail")]
        public async Task<ActionResult<PaymentOrderDetailResultDto>> PaymentOrderDetailQueryAsync([FromQuery] PaymentOrderDetailQueryRequest request)
        {
            return await _openBankingOperationHttpClient.PaymentOrderDetailQueryAsync(request);
        }

        /// <summary>
        /// This method is used to get card list
        /// </summary>
        /// <param name="query"></param>
        /// <returns>it returns consented card list .</returns>
        [Authorize(Policy = "OpenBankingOperation:Read")]
        [HttpGet("cards")]
        public async Task<ActionResult<CardsResultDto>> GetCardsAsync([FromQuery] GetCardsRequest request)
        {
            return await _openBankingOperationHttpClient.GetCardsAsync(request);
        }

        /// <summary>
        /// This method is used to get card detalis 
        /// </summary>
        /// <param name="query"></param>
        /// <returns>it returns card detail list .</returns>
        [Authorize(Policy = "OpenBankingOperation:Read")]
        [HttpGet("card-detail")]
        public async Task<ActionResult<CardDetailResultDto>> GetCardDetailAsync([FromQuery] GetCardDetailRequest request)
        {
            return await _openBankingOperationHttpClient.GetCardDetailAsync(request);
        }

        /// <summary>
        /// This method is used to get card detalis 
        /// </summary>
        /// <param name="query"></param>
        /// <returns>it returns consented card detail list .</returns>
        [Authorize(Policy = "OpenBankingOperation:Read")]
        [HttpGet("card-transactions")]
        public async Task<ActionResult<CardTransactionsResultDto>> GetCardTransactionAsync([FromQuery] GetCardTransactionsRequest request)
        {
            return await _openBankingOperationHttpClient.GetCardTransactionAsync(request);
        }

        /// <summary>
        /// This method is used to create future payment order consent
        /// </summary>
        /// <param name="request"></param>
        /// <returns>it returns FuturePaymentOrderConsentResultDto.</returns>
        [Authorize(Policy = "OpenBankingOperation:Create")]
        [HttpPost("create-future-payment-order-consent")]
        public async Task<ActionResult<FuturePaymentOrderConsentResultDto>> CreateFuturePaymentOrderConsentAsync([FromBody] CreateFuturePaymentOrderConsentRequest request)
        {
            return await _openBankingOperationHttpClient.CreateFuturePaymentOrderConsentAsync(request);
        }

        /// <summary>
        /// This method is used to list future payment orders
        /// </summary>
        /// <param name="request"></param>
        /// <returns>it returns cGetFuturePaymentOrderListResultDto.</returns>
        [Authorize(Policy = "OpenBankingOperation:Read")]
        [HttpPost("list-future-payment-order")]
        public async Task<ActionResult<GetFuturePaymentOrderListResultDto>> GetFuturePaymentOrderListAsync([FromBody] GetFuturePaymentOrderListRequest request)
        {
            return await _openBankingOperationHttpClient.GetFuturePaymentOrderListAsync(request);
        }

        /// <summary>
        /// This method is used to trigger future payment order
        /// </summary>
        /// <param name="request"></param>
        /// <returns>it returns TriggerFuturePaymentOrderResultDto.</returns>
        [Authorize(Policy = "OpenBankingOperation:Read")]
        [HttpPost("trigger-future-payment-order")]
        public async Task<ActionResult<TriggerFuturePaymentOrderResultDto>> TriggerFuturePaymentOrderAsync([FromBody] TriggerFuturePaymentOrderRequest request)
        {
            return await _openBankingOperationHttpClient.TriggerFuturePaymentOrderAsync(request);
        }

        /// <summary>
        /// This method is used to trigger future payment order
        /// </summary>
        /// <param name="request"></param>
        /// <returns>it returns CancelFuturePaymentOrderResultDto .</returns>
        [Authorize(Policy = "OpenBankingOperation:Create")]
        [HttpPost("cancel-future-payment-order")]
        public async Task<ActionResult<CancelFuturePaymentOrderResultDto>> CancelFuturePaymentOrderAsync([FromBody] CancelFuturePaymentOrderRequest request)
        {
            return await _openBankingOperationHttpClient.CancelFuturePaymentOrderAsync(request);
        }

        /// <summary>
        /// This method is used to create recurring payment order consent
        /// </summary>
        /// <param name="request"></param>
        /// <returns>it returns StandingPaymentOrderConsentResultDto</returns>
        [Authorize(Policy = "OpenBankingOperation:Create")]
        [HttpPost("create-standing-payment-order-consent")]
        public async Task<ActionResult<StandingPaymentOrderConsentResultDto>> CreateStandingPaymentOrderConsentAsync([FromBody] CreateStandingPaymentOrderConsentRequest request)
        {
            return await _openBankingOperationHttpClient.CreateStandingPaymentOrderConsentAsync(request);
        }
    }
}