using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.HttpClients
{
    public interface IOperationalTransferHttpClient
    {
        Task<OperationalTransferDto> GetOperationalTransferByIdAsync(Guid id);
        Task<PaginatedList<OperationalTransferDto>> GetOperationalTransferListAsync(GetOperationalTransferListRequest request);
    }
}
