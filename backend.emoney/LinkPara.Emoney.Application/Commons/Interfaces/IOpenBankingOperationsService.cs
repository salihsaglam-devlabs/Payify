
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

}
