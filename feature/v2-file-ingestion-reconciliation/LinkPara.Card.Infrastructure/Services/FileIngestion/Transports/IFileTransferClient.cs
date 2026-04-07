using LinkPara.Card.Application.Commons.Models.FileIngestion;
using LinkPara.Card.Domain.Enums.FileIngestion;
using System.Text;

namespace LinkPara.Card.Infrastructure.Services.FileIngestion.Transports;

public interface IFileTransferClient
{
    FileSourceType SourceType { get; }

    Task<IReadOnlyCollection<FileReference>> ListAsync(
        string profileKey,
        ProfileOptions profile,
        FileTransferEndpointType endpointType = FileTransferEndpointType.Source,
        CancellationToken cancellationToken = default);

    Task<Stream> OpenReadAsync(
        FileReference file,
        CancellationToken cancellationToken = default);

    Task<byte[]> ReadAllBytesAsync(
        FileReference file,
        CancellationToken cancellationToken = default);

    Task<string> ReadRangeAsync(
        FileReference file,
        long byteOffset,
        int byteLength,
        Encoding encoding,
        CancellationToken cancellationToken = default);

    Task<Stream> OpenWriteAsync(
        string profileKey,
        string fileName,
        FileTransferEndpointType endpointType = FileTransferEndpointType.Target,
        CancellationToken cancellationToken = default);

    Task<FileReference> WriteAllBytesAsync(
        string profileKey,
        string fileName,
        ReadOnlyMemory<byte> content,
        FileTransferEndpointType endpointType = FileTransferEndpointType.Target,
        CancellationToken cancellationToken = default);
}
