using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.Merchant.Services.Pf.HttpClients;

public interface IMerchantContentHttpClient
{
    Task<PaginatedList<MerchantContentDto>> GetAllMerchantContentAsync(GetFilterMerchantContentRequest request);
    Task<MerchantContentDto> GetMerchantContentByIdAsync(Guid id);
    Task CreateMerchantContentAsync(CreateMerchantContentRequest request);
    Task PutMerchantContentAsync(MerchantContentDto request);
    Task DeleteMerchantContentAsync(Guid id);
    Task<MerchantLogoDto> GetMerchantLogoAsync(Guid merchantId);
    Task UploadMerchantLogoAsync(MerchantLogoDto merchantLogo);
}