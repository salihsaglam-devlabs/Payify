using LinkPara.PF.Application.Features.PhysicalPos.UnacceptableTransaction;
using LinkPara.PF.Application.Features.PhysicalPos.UnacceptableTransaction.Command.UpdateUnacceptableTransaction;
using LinkPara.PF.Application.Features.PhysicalPos.UnacceptableTransaction.Queries.GetAllUnacceptableTransaction;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.PF.Application.Commons.Interfaces.PhysicalPos;

public interface IUnacceptableTransactionService
{
    Task RetryUnacceptableTransactionAsync(Guid unacceptableTransactionId);
    Task<PaginatedList<PhysicalPosUnacceptableTransactionDto>> GetAllUnacceptableTransactionsAsync(GetAllUnacceptableTransactionQuery request);
    Task<UnacceptableTransactionDetailResponse> GetDetailsByIdAsync(Guid id);
    Task<PhysicalPosUnacceptableTransactionDto> UpdateStatusAsync(UpdateUnacceptableTransactionCommand command);
}