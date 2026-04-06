
using LinkPara.Emoney.Application.Features.OpenBankingOperations;
using LinkPara.Emoney.Application.Features.OpenBankingOperations.Commands.CreateAccountConsent;
using LinkPara.Emoney.Application.Features.OpenBankingOperations.Commands.DeleteAccountConsent;
using LinkPara.Emoney.Application.Features.OpenBankingOperations.Queries.GetHhsAccessToken;
using LinkPara.Emoney.Application.Features.OpenBankingOperations.Queries.GetAccountConsent;
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
using LinkPara.Emoney.Application.Features.OpenBankingOperations.Queries.GetCards;
using LinkPara.Emoney.Application.Features.OpenBankingOperations.Queries.GetCardDetail;
using LinkPara.Emoney.Application.Features.OpenBankingOperations.Queries.GetCardTransactions;
using LinkPara.Emoney.Application.Features.OpenBankingOperations.Commands.CreateFuturePaymentOrderConsent;
using LinkPara.Emoney.Application.Features.OpenBankingOperations.Commands.TriggerFuturePaymentOrder;
using Microsoft.AspNetCore.Mvc;
using LinkPara.Emoney.Application.Features.OpenBankingOperations.Commands.CancelFuturePaymentOrder;
using LinkPara.Emoney.Application.Features.OpenBankingOperations.Commands.CreateStandingPaymentOrderConsent;

namespace LinkPara.Emoney.Application.Commons.Interfaces;

public interface IOpenBankingOperationsService
{
    Task<HhsResultDto> GetHhsListAsync();
    Task<AccountConsentDetailResultDto> CreateAccountConsentAsync(CreateAccountConsentCommand command);
    Task<YosServiceResultDto> GetHhsAccessTokenAsync(GetHhsAccessTokenQuery query);
    Task<AccountConsentDetailResultDto> GetAccountConsentDetailAsync(GetAccountConsentQuery query);
    Task<YosServiceResultDto> DeleteAccountConsentAsync(DeleteAccountConsentCommand command);
    Task<ActiveAccountConsentResultDto> GetActiveAccountConsentListAsync(GetActiveAccountConsentListQuery query);
    Task<ConsentedAccountsResultDto> GetConsentedAccountListAsync(GetConsentedAccountListQuery query);
    Task<ConsentedAccountDetailResultDto> GetConsentedAccountDetailAsync(GetConsentedAccountDetailQuery query);
    Task<ConsentedAccountBalancesResultDto> GetConsentedAccountBalanceListAsync(GetConsentedAccountBalanceListQuery query);
    Task<ConsentedAccountBalanceDetailResultDto> GetConsentedAccountBalanceDetailAsync(GetConsentedAccountBalanceDetailQuery query);
    Task<ConsentedAccountActivitiesResultDto> GetConsentedAccountActivitiesAsync(GetConsentedAccountActivitiesQuery query);
    Task<PaymentOrderConsentDetailDto> CreatePaymentConsentAsync(CreatePaymentConsentCommand command);
    Task<PaymentOrderConsentDetailDto> GetPaymentOrderConsentDetailAsync(GetPaymentOrderConsentDetailQuery query);
    Task<PaymentOrderDetailResultDto> CreatePaymentOrderAsync(CreatePaymentOrderYosCommand command);
    Task<PaymentOrderDetailResultDto> PaymentOrderDetailQueryAsync(PaymentOrderDetailQuery query);
    Task<CardsResultDto> GetCardsAsync(GetCardsQuery query);
    Task<CardDetailResultDto> GetCardDetailAsync(GetCardDetailQuery query);
    Task<CardTransactionsResultDto> GetCardTransactionsAsync(GetCardTransactionsQuery query);
    Task<FuturePaymentOrderConsentResultDto> CreateFuturePaymentOrderConsentAsync(CreateFuturePaymentOrderConsentCommand command);
    Task<StandingPaymentOrderConsentResultDto> CreateStandingPaymentOrderConsentAsync(CreateStandingPaymentOrderConsentCommand command);
    Task<TriggerFuturePaymentOrderResultDto> TriggerFuturePaymentOrderAsync(TriggerFuturePaymentOrderCommand command);
    Task<GetFuturePaymentOrderListResultDto> GetFuturePaymentOrderListAsync([FromBody] GetFuturePaymentOrderListQuery query);
    Task<CancelFuturePaymentOrderResultDto> CancelFuturePaymentOrderAsync([FromBody] CancelFuturePaymentOrderCommand command);

}
