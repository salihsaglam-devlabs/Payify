using LinkPara.Epin.Application.Commons.Models.Perdigital.Requests.Order;
using LinkPara.Epin.Application.Commons.Models.Perdigital.Responses.Order;
using LinkPara.Epin.Application.Commons.Models.Perdigital.Responses.Balance;
using LinkPara.Epin.Application.Commons.Models.Perdigital.Responses.Brand;
using LinkPara.Epin.Application.Commons.Models.Perdigital.Responses.Product;
using LinkPara.Epin.Application.Commons.Models.Perdigital.Responses.Publisher;
using LinkPara.Epin.Application.Commons.Models.Perdigital.Responses.Reconciliation;
using LinkPara.Epin.Application.Commons.Models.Perdigital.Responses.OrderHistory;

namespace LinkPara.Epin.Application.Commons.Interfaces;

public interface IEpinHttpClient
{
    Task<PublisherServiceResponse> GetPublishersAsync();
    Task<BrandServiceResponse> GetBrandsAsync(int publisherId);
    Task<ProductServiceResponse> GetProductsAsync(int publisherId, int brandId);
    Task<BalanceServiceResponse> GetBalanceAsync();
    Task<bool> CheckStockAsync(int productId);
    Task<OrderServiceResponse> CreateOrderAsync(OrderServiceRequest request);
    Task<ReconciliationServiceResponse> GetReconciliationResultByDateAsync(DateTime date);
    Task ReconciliationApproveByDateAsync(DateTime date);
    Task<OrderHistoryServiceResponse> GetOrderHistoryByDateAsync(DateTime date);
}
