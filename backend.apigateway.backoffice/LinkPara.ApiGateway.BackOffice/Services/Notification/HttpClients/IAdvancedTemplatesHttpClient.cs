using LinkPara.ApiGateway.BackOffice.Services.Notification.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Notification.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Services.Notification.HttpClients;

public interface IAdvancedTemplatesHttpClient
{
    Task CreateAdvancedTemplateAsync(CreateAdvancedTemplateRequest request);
    Task UpdateAdvancedTemplateAsync(UpdateAdvancedTemplateRequest request);
    Task<PaginatedList<TemplateDto>> GetAllAsync(GetTemplatesFilterRequest request);
    Task<PaginatedList<TemplateContentDto>> GetFilterContentsAsync(GetFilterContentsRequest request);
    Task<DefaultTemplateDto> GetDefaultTemplate(GetDefaultTemplateRequest request);
    Task DeleteTemplateById(Guid templateId);
    Task<TemplateDto> GetTemplateById(Guid templateId);
}