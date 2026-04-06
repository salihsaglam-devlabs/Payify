using LinkPara.Fraud.Application.Commons.Models.Transactions.SanctionScanners.Response;
using LinkPara.Fraud.Application.Features.Transactions;
using LinkPara.Fraud.Application.Features.Transactions.Queries.GetAllTransactions;
using LinkPara.HttpProviders.Fraud.Models;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.Fraud.Application.Commons.Interfaces;

public interface ITransactionService
{
    Task<TransactionApiResponse> GetTransactionAsync(string transactionId);
    Task<TransactionDetailResponse> GetTransactionDetailAsync(string transactionId);
    Task<CancelTransactionResponse> CancelTransactionAsync(string transactionId);
    Task<TransactionResponse> ExecuteTransactionAsync(FraudCheckRequest request);
    Task<PaginatedList<TransactionMonitoringDto>> GetListAsync(GetAllTransactionQuery request);
    Task<string> GetTriggeredRuleSetKey(string operation, string userLevel, bool isCompliance);
}
