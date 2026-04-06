using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.EMoney.HttpClients;

public interface ICompanyPoolHttpClient
{
    Task ApproveCompanyPoolAsync(ApproveCompanyPoolRequest request);
    Task<List<CompanyDocumentTypeResponse>> GetCompanyDocumentTypesAsync(GetCompanyDocumentTypesRequest request);
    Task<CompanyPoolDto> GetCompanyPoolAsync(Guid id);
    Task<PaginatedList<CompanyPoolDto>> GetCompanyPoolsListAsync(GetCompanyPoolListRequest request);
    Task<SaveCompanyPoolResponse> SaveCompanyPoolAsync(SaveCompanyPoolRequest request);
}
