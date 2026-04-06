using LinkPara.ApiGateway.BackOffice.Services.Notification.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Notification.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Notification.HttpClients
{
    public interface INotificationTemplatesHttpClient
    {
        Task<NotificationTemplateDto> GetNotificationTemplateById(Guid templateId);
        Task<PaginatedList<NotificationTemplateDto>> GetAllNotificationTemplates(TemplateFilterQuery request);
        Task CreateNotificationTemplate(CreateNotificationTemplateRequest request);
        Task UpdateNotificationTemplate(UpdateNotificationTemplateRequest request);
        Task DeleteNotificationTemplate(Guid templateId);
    }
}
