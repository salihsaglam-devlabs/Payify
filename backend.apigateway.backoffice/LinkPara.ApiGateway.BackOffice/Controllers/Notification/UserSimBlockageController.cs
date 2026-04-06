using LinkPara.ApiGateway.BackOffice.Services.Notification.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.Notification.Models;
using LinkPara.ApiGateway.BackOffice.Services.Notification.Models.Requests;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace LinkPara.ApiGateway.BackOffice.Controllers.Notification;

public class UserSimBlockageController : ApiControllerBase
{
    private readonly IUserSimBlockageHttpClient _httpClient;
    public UserSimBlockageController(IUserSimBlockageHttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    /// <summary>
    /// Gets User Sim Blockage List
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "SimBlockage:ReadAll")]
    [HttpGet("")]
    public async Task<PaginatedList<UserSimBlockageDto>> GetUserSimBlockageListAsync([FromQuery] GetUserSimBlockageRequest request)
    {
        return await _httpClient.GetUserSimBlockageListAsync(request);
    }

    /// <summary>
    /// Removes User Sim Blockage
    /// </summary>
    /// <param name="file"></param>
    /// <param name="documentTypeId"></param>
    /// <param name="phoneNumber"></param>
    [Authorize(Policy = "SimBlockage:Update")]
    [HttpPut("")]
    public async Task RemoveUserSimBlockageAsync([Required] IFormFile file,[Required] Guid documentTypeId, [Required] string phoneNumber)
    {
        await using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);

        var request = new RemoveUserSimBlockageRequest
        {
            Bytes = memoryStream.ToArray(),
            ContentType = file.ContentType,
            OriginalFileName = file.FileName,
            PhoneNumber = phoneNumber,
            DocumentTypeId = documentTypeId
        };

        await _httpClient.RemoveUserSimBlockageAsync(request);
    }
}
