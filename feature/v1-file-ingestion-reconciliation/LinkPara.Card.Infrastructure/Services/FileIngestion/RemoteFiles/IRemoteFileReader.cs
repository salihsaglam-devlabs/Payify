using System.Text;

namespace LinkPara.Card.Infrastructure.Services.FileIngestion.RemoteFiles;

public interface IRemoteFileReader
{
    Task<IReadOnlyCollection<string>> ListFilesAsync(string incomingPath, CancellationToken cancellationToken);
    Task<string> ReadFileTextAsync(string incomingPath, string fileName, Encoding encoding, CancellationToken cancellationToken);
}
