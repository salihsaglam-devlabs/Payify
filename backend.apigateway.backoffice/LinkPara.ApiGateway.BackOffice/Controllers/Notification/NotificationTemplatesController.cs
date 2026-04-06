using LinkPara.ApiGateway.BackOffice.Services.Notification.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.Notification.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Notification.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Controllers.Notification
{
    public class NotificationTemplatesController : ApiControllerBase
    {
        INotificationTemplatesHttpClient _httpClient;

        public NotificationTemplatesController(INotificationTemplatesHttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        /// <summary>
        /// Returns all Notification templates.
        /// </summary>
        /// <returns></returns>
        [HttpGet("")]
        [Authorize(Policy = "Templates:ReadAll")]
        public async Task<ActionResult<PaginatedList<NotificationTemplateDto>>> GetAllAsync([FromQuery] TemplateFilterQuery request)
        {
            return await _httpClient.GetAllNotificationTemplates(request);
        }

        /// <summary>
        /// Returns a notification template.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [Authorize(Policy = "Templates:Read")]
        public async Task<ActionResult<NotificationTemplateDto>> GetByIdAsync([FromRoute] Guid id)
        {
            return await _httpClient.GetNotificationTemplateById(id);
        }

        /// <summary>
        /// Creates a notification template.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("")]
        [Authorize(Policy = "Templates:Create")]
        public async Task SaveAsync(CreateNotificationTemplateRequest request)
        {
            await _httpClient.CreateNotificationTemplate(request);
        }

        /// <summary>
        /// Updates an notification template.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut("")]
        [Authorize(Policy = "Templates:Update")]
        public async Task UpdateAsync(UpdateNotificationTemplateRequest request)
        {
            await _httpClient.UpdateNotificationTemplate(request);
        }

        /// <summary>
        /// Delete a notification template.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [Authorize(Policy = "Templates:Delete")]
        public async Task DeleteAsync([FromRoute] Guid id)
        {
            await _httpClient.DeleteNotificationTemplate(id);
        }
    }
}
