using LinkPara.ApiGateway.BackOffice.Services.DigitalKyc.Models;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.DigitalKyc.HttpClients;
public interface IScSoftHttpClient
{
    Task<PaginatedList<CustomerInformationResponse>> GetAllCustomerInformationsAsync(GetAllCustomerInformationsRequest request);
    Task<CustomerInformationResponse> GetCustomerInformationByIdAsync(Guid id);
}
