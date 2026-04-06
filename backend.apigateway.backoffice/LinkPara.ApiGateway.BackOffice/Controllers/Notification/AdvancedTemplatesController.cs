using LinkPara.ApiGateway.BackOffice.Services.Notification.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.Notification.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Notification.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Controllers.Notification;

public class AdvancedTemplatesController : ApiControllerBase
{
    private readonly IAdvancedTemplatesHttpClient _httpClient;
    
    public AdvancedTemplatesController(IAdvancedTemplatesHttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    /// <summary>
    /// Returns all templates.
    /// </summary>
    /// <returns></returns>
    [HttpGet("")]
    [Authorize(Policy = "Templates:ReadAll")]
    public async Task<ActionResult<PaginatedList<TemplateDto>>> GetAllAsync([FromQuery] GetTemplatesFilterRequest request)
    {
        return await _httpClient.GetAllAsync(request);
    }
    
    /// <summary>
    /// Returns all templates.
    /// </summary>
    /// <returns></returns>
    [HttpGet("contents")]
    [Authorize(Policy = "Templates:ReadAll")]
    public async Task<ActionResult<PaginatedList<TemplateContentDto>>> GetFilterContentsAsync([FromQuery] GetFilterContentsRequest request)
    {
        return await _httpClient.GetFilterContentsAsync(request);
    }

    /// <summary>
    /// Returns a default template for given event name.
    /// </summary>
    /// <returns></returns>
    [HttpGet("default")]
    [Authorize(Policy = "Templates:Read")]
    public async Task<ActionResult<DefaultTemplateDto>> GetDefaultTemplateAsync([FromQuery] GetDefaultTemplateRequest request)
    {
        return await _httpClient.GetDefaultTemplate(request);
    }

    /// <summary>
    /// Creates a template.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("")]
    [Authorize(Policy = "Templates:Create")]
    public async Task SaveAsync(CreateAdvancedTemplateRequest request)
    {
        await _httpClient.CreateAdvancedTemplateAsync(request);
    }

    /// <summary>
    /// Updates a template.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPut("")]
    [Authorize(Policy = "Templates:Update")]
    public async Task UpdateAsync(UpdateAdvancedTemplateRequest request)
    {
        await _httpClient.UpdateAdvancedTemplateAsync(request);
    }

    /// <summary>
    /// Deletes a template.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    [Authorize(Policy = "Templates:Delete")]
    public async Task DeleteAsync([FromRoute] Guid id)
    {
        await _httpClient.DeleteTemplateById(id);
    }
    
    /// <summary>
    /// Deletes a template.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [Authorize(Policy = "Templates:Read")]
    public async Task<TemplateDto> GetById([FromRoute] Guid id)
    {
        return await _httpClient.GetTemplateById(id);
    }
}