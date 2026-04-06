using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.HttpClients;

public interface ICompanyIbanHttpClient
{
    Task<CompanyIbanDto> GetByIdAsync(Guid id);
    Task<PaginatedList<CompanyIbanDto>> GetListAsync(GetCompanyIbanListRequest request);
    Task SaveAsync(SaveCompanyIbanRequest request);
    Task UpdateAsync(UpdateCompanyIbanRequest request);
    Task DeleteAsync(DeleteCompanyIbanRequest request);
}
