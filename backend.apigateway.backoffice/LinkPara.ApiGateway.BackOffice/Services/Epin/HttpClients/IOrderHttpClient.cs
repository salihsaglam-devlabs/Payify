using LinkPara.ApiGateway.BackOffice.Services.Epin.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Epin.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Epin.HttpClients;

public interface IOrderHttpClient
{
    Task CancelOrderAsync(Guid orderId);
    Task<PaginatedList<OrderDto>> GetOrdersFilterAsync(GetOrdersFilterRequest request);
    Task<OrderSummaryDto> GetOrderSummaryAsync(Guid id);
}
