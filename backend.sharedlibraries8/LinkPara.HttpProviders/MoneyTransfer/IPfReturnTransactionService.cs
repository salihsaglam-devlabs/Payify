using LinkPara.HttpProviders.MoneyTransfer.Models;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.HttpProviders.MoneyTransfer;
public interface IPfReturnTransactionService
{
    Task<PaginatedList<PfReturnTransactionDto>> GetListAsync(GetPfReturnTransactionsRequest request);
    Task<VerifyPfReturnTransactionResponse> VerifyPfReturnTransactionAsync(VerifyPfReturnTransactionRequest request);
    Task<bool> ReturnPfTransactionAsync(ReturnPfTransactionRequest request);
}
