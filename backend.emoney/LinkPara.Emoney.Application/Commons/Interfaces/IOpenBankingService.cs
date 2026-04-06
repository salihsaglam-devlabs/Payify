using LinkPara.Emoney.Application.Features.AccountServiceProviders;
using LinkPara.Emoney.Application.Features.AccountServiceProviders.Commands.CreatePaymentOrder;
using LinkPara.Emoney.Application.Features.AccountServiceProviders.Commands.SendGkdNotification;
using LinkPara.Emoney.Application.Features.AccountServiceProviders.Commands.SendOtpMessage;
using LinkPara.Emoney.Application.Features.AccountServiceProviders.Queries.GetAccountTransactions;
using LinkPara.Emoney.Application.Features.AccountServiceProviders.Queries.GetChangedBalance;
using LinkPara.Emoney.Application.Features.AccountServiceProviders.Queries.PaymentOrderInquiry;

namespace LinkPara.Emoney.Application.Commons.Interfaces;

public interface IOpenBankingService
{
    Task<AccountTransactionsDto> GetAccountTransactionsAsync(GetAccountTransactionsQuery query);
    Task<List<ChangedBalanceDto>> GetChangedBalanceAsync(GetChangedBalanceQuery query);
    Task<SendNotificationResultDto> SendGkdNotificationAsync(SendGkdNotificationCommand command);
    Task<SendOtpMessageResultDto> SendOtpMessageAsync(SendOtpMessageCommand command);
    Task<PaymentContractDto> CreatePaymentOrderAsync(CreatePaymentOrderCommand command, CancellationToken cancellationToken);
    Task<PaymentContractDto> PaymentOrderInquiryAsync(PaymentOrderInquiryQuery query);
    Task CheckChangedBalanceAsync();
}
