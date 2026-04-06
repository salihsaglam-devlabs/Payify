using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.Merchant.Services.Pf.HttpClients;

public interface IMerchantTransactionHttpClient
{
    Task<PaginatedList<MerchantTransactionDto>> GetAllAsync(GetAllMerchantTransactionRequest request);
    Task<MerchantTransactionDto> GetByIdAsync(Guid id);
    Task<List<MerchantTransactionStatusModel>> GetStatusCountAsync(MerchantTransactionStatusRequest request);
    Task<string> GenerateOrderNumberAsync(Guid merchantId);
    Task<OrderNumberResponse> GenerateUniqueOrderNumberAsync(OrderNumberRequest request);
}
