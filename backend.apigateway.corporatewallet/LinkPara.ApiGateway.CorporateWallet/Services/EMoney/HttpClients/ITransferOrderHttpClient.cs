using LinkPara.ApiGateway.CorporateWallet.Services.Emoney.Models.Requests;
using LinkPara.ApiGateway.CorporateWallet.Services.Emoney.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.CorporateWallet.Services.Emoney.HttpClients;

public interface ITransferOrderHttpClient
{
    Task<TransferOrderDto> GetTransferOrderByIdAsync(Guid transferOrderId);
    Task<PaginatedList<TransferOrderDto>> GetTransferOrdersByUserIdAsync(GetTransferOrdersRequest query);
    Task CreateTransferOrderAsync(CreateTransferOrderRequest request);
    Task UpdateTransferOrderAsync(Guid transferOrderId, UpdateTransferOrderRequest request);
    Task DeleteTransferOrderAsync(Guid transferOrderId);
}
