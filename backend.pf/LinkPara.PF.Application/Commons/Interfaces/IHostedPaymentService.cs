using LinkPara.PF.Application.Features.HostedPayments;
using LinkPara.PF.Application.Features.HostedPayments.Command.InitHostedPayment;
using LinkPara.PF.Application.Features.HostedPayments.Command.SaveHostedPayment;
using LinkPara.PF.Application.Features.HostedPayments.Queries.GetHppTransactions;
using LinkPara.PF.Domain.Entities;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.PF.Application.Commons.Interfaces;

public interface IHostedPaymentService
{
    Task<InitHostedPaymentResponse> InitHostedPaymentAsync(InitHostedPaymentCommand request);
    Task<HostedPaymentResponse> SaveHostedPaymentAsync(SaveHostedPaymentCommand request);
    Task<HppTransactionResponse> GetHppTransactionAsync(string trackingId, Guid merchantId);
    Task TriggerHppWebhookAsync(string trackingId);
    Task<PaginatedList<HostedPaymentDto>> GetFilterListAsync(GetHppTransactionsQuery request);
}