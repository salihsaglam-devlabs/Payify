using LinkPara.Billing.Application.Commons.Models.Reconciliation;
using LinkPara.Billing.Application.Features.Billing;
using LinkPara.Billing.Application.Features.Billing.Queries.GetBillInfoByConversationId;
using LinkPara.Billing.Application.Features.Billing.Queries.GetBillTransactions;
using LinkPara.Billing.Domain.Entities;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.Billing.Application.Commons.Interfaces;

public interface ITransactionService
{
    Task<Transaction> GetByIdAsync(Guid transactionId);
    Task AddAsync(Transaction transaction);
    Task UpdateAsync(Transaction transaction);
    Task<PaginatedList<BillTransactionResponseDto>> GetListAsync(GetBillTransactionsQuery request);
    Task<BillTransactionResponseDto> GetBillInfoByConversationIdAsync(GetBillInfoByConversationIdQuery request);
    Task<TransactionStatistics> GetTransactionStatisticsAsync(Guid vendorId, DateTime paymentDate);
    Task<List<TransactionStatistics>> GetTransactionStatisticsByInstitutionAsync(Guid vendorId, Guid institutionId, DateTime paymentDate);
    Task<List<Transaction>> GetReconciliableInstitutionTransactionsForVendor(Guid institutionId, Guid vendorId, DateTime paymentDate);
}