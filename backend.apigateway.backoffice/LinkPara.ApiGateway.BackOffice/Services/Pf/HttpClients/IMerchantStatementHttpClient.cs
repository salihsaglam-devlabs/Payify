using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients;

public interface IMerchantStatementHttpClient
{
    Task<PaginatedList<MerchantStatementDto>> GetMerchantStatementsAsync(GetFilterMerchantStatementRequest request);    
    Task<HttpResponseMessage> GetMerchantStatementWithBytesAsync(DownloadMerchantStatementRequest request);
}
