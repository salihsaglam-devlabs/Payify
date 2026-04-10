#nullable enable
using LinkPara.Card.Application.Commons.Models.Archive.Contracts.Responses;

namespace LinkPara.Card.Application.Commons.Interfaces.Archive;

public interface IArchiveErrorMapper
{
    ArchiveErrorDetail MapException(
        Exception ex,
        string step,
        Guid? ingestionFileId = null,
        string? detail = null,
        string? message = null);

    ArchiveErrorDetail Create(
        string code,
        string message,
        string step,
        Guid? ingestionFileId = null,
        string? detail = null,
        string severity = "Error");
}

