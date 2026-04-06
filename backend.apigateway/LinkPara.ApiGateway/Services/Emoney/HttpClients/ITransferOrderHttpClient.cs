using LinkPara.ApiGateway.Services.Emoney.Models.Requests;
using LinkPara.ApiGateway.Services.Emoney.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.Services.Emoney.HttpClients;

public interface ITransferOrderHttpClient
{
    Task<TransferOrderDto> GetTransferOrderByIdAsync(Guid transferOrderId);
    Task<PaginatedList<TransferOrderDto>> GetTransferOrdersByUserIdAsync(GetTransferOrdersRequest query);
    Task CreateTransferOrderAsync(CreateTransferOrderRequest request);
    Task UpdateTransferOrderAsync(Guid transferOrderId, UpdateTransferOrderRequest request);
    Task DeleteTransferOrderAsync(Guid transferOrderId);
}
