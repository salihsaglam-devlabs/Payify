using LinkPara.Cache;
using LinkPara.Content.Application.Features.Contents.Commands.CreateDataContainer;
using LinkPara.Content.Application.Features.Contents.Commands.DeleteDataContainer;
using LinkPara.Content.Application.Features.Contents.Commands.UpdateDataContainer;
using LinkPara.Content.Application.Features.Contents.Queries;
using LinkPara.Content.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.Content.API.Controllers;
public class DataContainersController : ApiControllerBase
{
    private readonly ICacheService _cacheService;
    private readonly IConfiguration _configuration;
    public DataContainersController(ICacheService cacheService, IConfiguration configuration)
    {
        _cacheService = cacheService;
        _configuration = configuration;
    }
    [Authorize(Policy = "Content:Read")]
    [HttpGet("{Key}")]
    public async Task<DataContainer> GetContentAsync([FromRoute] GetDataContainerQuery query)
    {
        var dataFromCache = _cacheService.Get<DataContainer>(query.Key);
        if (dataFromCache != null)
        {
            return dataFromCache;
        }

        var data = await Mediator.Send(query);

        _cacheService.Add(query.Key, data, GetCacheExpirationInMinutes());

        return data;
    }

    [Authorize(Policy = "Content:Create")]
    [HttpPost("")]
    public async Task CreateContentAsync(CreateDataContainerCommand command)
    {
        await Mediator.Send(command);
        _cacheService.Add(command.Key, command.Value, GetCacheExpirationInMinutes());
    }

    [Authorize(Policy = "Content:Update")]
    [HttpPut("{key}")]
    public async Task UpdateContentAsync(string key, UpdateDataContainerCommand command)
    {
        command.Key = key;
        await Mediator.Send(command);
        _cacheService.Add(command.Key, command.Value, GetCacheExpirationInMinutes());
    }

    [Authorize(Policy = "Content:Delete")]
    [HttpDelete("{Key}")]
    public async Task DeleteContentAsync([FromRoute] DeleteDataContainerCommand command)
    {
        await Mediator.Send(command);
    }

    private int GetCacheExpirationInMinutes()
    {
        return int.Parse(_configuration["ResponseCacheExpirationMinutes"]);
    }
}