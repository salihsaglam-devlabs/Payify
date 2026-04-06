using LinkPara.ApiGateway.BackOffice.Services.Identity.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Identity.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Identity.HttpClients
{
    public interface IQuestionHttpClient
    {
        Task<PaginatedList<SecurityQuestionDto>> GetAllSecurityQuestionAsync(GetAllSecurityQuestionRequest request);
        Task SaveAsync(SecurityQuestionRequest request);
        Task UpdateAsync(UpdateSecurityQuestionRequest request);
        Task DeleteSecurityQuestionAsync(Guid id);
        Task<SecurityQuestionDto> GetSecurityQuestionByIdAsync(Guid id);
    }
}
