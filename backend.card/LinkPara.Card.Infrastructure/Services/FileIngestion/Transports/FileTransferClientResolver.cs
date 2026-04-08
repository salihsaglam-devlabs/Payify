using LinkPara.Card.Application.Commons.Models.FileIngestion;
using LinkPara.Card.Application.Commons.Interfaces.Localization;
using LinkPara.Card.Domain.Enums.FileIngestion;
using Microsoft.Extensions.Options;

namespace LinkPara.Card.Infrastructure.Services.FileIngestion.Transports;

public class FileTransferClientResolver : IFileTransferClientResolver
{
    private readonly IEnumerable<IFileTransferClient> _clients;
    private readonly FileIngestionOptions _options = new();
    private readonly ICardResourceLocalizer _localizer;

    public FileTransferClientResolver(
        IEnumerable<IFileTransferClient> clients,
        IOptions<FileIngestionOptions> options,
        ICardResourceLocalizer localizer)
    {
        _clients = clients;
        _options = options.Value;
        _localizer = localizer;
    }

    public IFileTransferClient Create(
        FileSourceType sourceType,
        FileTransferEndpointType endpointType = FileTransferEndpointType.Source)
    {
        if (sourceType == FileSourceType.Local)
            return _clients.Single(x => x.SourceType == FileSourceType.Local);

        var endpoint = endpointType == FileTransferEndpointType.Target
            ? _options.Connections.Target
            : _options.Connections.Source;

        if (string.Equals(endpoint.Protocol, "Sftp", StringComparison.OrdinalIgnoreCase))
            return _clients.Single(x => x.GetType() == typeof(SftpFileTransferClient));

        if (string.Equals(endpoint.Protocol, "Ftp", StringComparison.OrdinalIgnoreCase))
            return _clients.Single(x => x.GetType() == typeof(FtpFileTransferClient));

        if (string.Equals(endpoint.Protocol, "Local", StringComparison.OrdinalIgnoreCase))
            return _clients.Single(x => x.SourceType == FileSourceType.Local);

        throw new InvalidOperationException(_localizer.Get("FileIngestion.UnsupportedProtocol", endpoint.Protocol));
    }
}
