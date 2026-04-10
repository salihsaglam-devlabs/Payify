using LinkPara.Card.Application.Commons.Models.FileIngestion;
using LinkPara.Card.Application.Commons.Models.FileIngestion.Shared;
using LinkPara.Card.Application.Features.FileIngestion.Commands.IngestFile;
using LinkPara.Card.Domain.Enums.FileIngestion;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.Card.API.Controllers;

public class FileIngestionController : ApiControllerBase
{
    [Authorize(Policy = FileIngestionPolicies.Create)]
    [HttpPost("")]
    public Task<List<FileIngestionResponse>> IngestAsync([FromBody] FileIngestionRequest request)
    {
        return Mediator.Send(MapToCommand(request ?? new FileIngestionRequest()));
    }

    [Authorize(Policy = FileIngestionPolicies.Create)]
    [HttpPost("{fileSourceType}/{fileType}/{fileContentType}")]
    public Task<List<FileIngestionResponse>> IngestByRouteAsync(
        FileSourceType fileSourceType,
        FileType fileType,
        FileContentType fileContentType,
        [FromBody] FileIngestionRouteRequest request)
    {
        return Mediator.Send(new FileIngestionCommand
        {
            FileSourceType = fileSourceType,
            FileType = fileType,
            FileContentType = fileContentType,
            FilePath = request?.FilePath
        });
    }

    private static FileIngestionCommand MapToCommand(FileIngestionRequest request)
    {
        return new FileIngestionCommand
        {
            FileSourceType = request.FileSourceType,
            FileType = request.FileType,
            FileContentType = request.FileContentType,
            FilePath = request.FilePath
        };
    }
}
