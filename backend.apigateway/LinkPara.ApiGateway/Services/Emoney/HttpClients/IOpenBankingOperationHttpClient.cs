using LinkPara.ApiGateway.OpenBanking.Services.Emoney.Responses;
using LinkPara.ApiGateway.Services.Emoney.Models.Requests;
using LinkPara.ApiGateway.Services.Emoney.Models.Responses;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.Services.Emoney.HttpClients
{
    public interface IOpenBankingOperationHttpClient
    {
        Task<HhsResultDto> GetHhsListAsync();
        Task<AccountConsentDetailResultDto> CreateAccountConsentAsync(CreateAccountConsentRequest request);
        Task<YosServiceResultDto> GetHhsAccessTokenAsync(GetHhsAccessTokenRequest request);
        Task<AccountConsentDetailResultDto> GetAccountConsentDetailAsync(string consentId);
        Task<YosServiceResultDto> DeleteAccountConsentAsync(string consentId);
        Task<ActiveAccountConsentResultDto> GetActiveAccountConsentListAsync(GetActiveAccountConsentListRequest request);
        Task<ConsentedAccountsResultDto> GetConsentedAccountListAsync(GetConsentedAccountListRequest request);
        Task<ConsentedAccountDetailResultDto> GetConsentedAccountDetailAsync(GetConsentedAccountDetailRequest request);
        Task<ConsentedAccountBalancesResultDto> GetConsentedAccountBalanceListAsync(GetConsentedAccountBalanceListRequest request);
        Task<ConsentedAccountBalanceDetailResultDto> GetConsentedAccountBalanceDetailAsync(GetConsentedAccountBalanceDetailRequest request);
        Task<ConsentedAccountActivitiesResultDto> GetConsentedAccountActivitiesAsync(GetConsentedAccountActivitiesRequest request);
        Task<PaymentOrderConsentDetailDto> CreatePaymentConsentAsync(CreatePaymentConsentRequest request);
        Task<PaymentOrderConsentDetailDto> GetPaymentOrderConsentDetailAsync(GetPaymentOrderConsentDetailRequest request);
        Task<PaymentOrderDetailResultDto> CreatePaymentOrderAsync(CreatePaymentOrderRequest request);
        Task<PaymentOrderDetailResultDto> PaymentOrderDetailQueryAsync(PaymentOrderDetailQueryRequest request);
        Task<ActionResult<CardsResultDto>> GetCardsAsync([FromQuery] GetCardsRequest request);
        Task<ActionResult<CardDetailResultDto>> GetCardDetailAsync([FromQuery] GetCardDetailRequest request);
        Task<ActionResult<FuturePaymentOrderConsentResultDto>> CreateFuturePaymentOrderConsentAsync([FromBody] CreateFuturePaymentOrderConsentRequest request);
        Task<ActionResult<GetFuturePaymentOrderListResultDto>> GetFuturePaymentOrderListAsync([FromBody] GetFuturePaymentOrderListRequest request);
        Task<ActionResult<TriggerFuturePaymentOrderResultDto>> TriggerFuturePaymentOrderAsync([FromBody] TriggerFuturePaymentOrderRequest request);
        Task<ActionResult<CancelFuturePaymentOrderResultDto>> CancelFuturePaymentOrderAsync([FromBody] CancelFuturePaymentOrderRequest request);
        Task<ActionResult<StandingPaymentOrderConsentResultDto>> CreateStandingPaymentOrderConsentAsync([FromBody] CreateStandingPaymentOrderConsentRequest request);
        Task<ActionResult<CardTransactionsResultDto>> GetCardTransactionAsync([FromQuery] GetCardTransactionsRequest request);

    }
}