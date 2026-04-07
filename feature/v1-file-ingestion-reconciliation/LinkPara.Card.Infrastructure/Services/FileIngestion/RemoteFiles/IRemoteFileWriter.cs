namespace LinkPara.Card.Infrastructure.Services.FileIngestion.RemoteFiles;

public interface IRemoteFileWriter
{
    Task<bool> IsEnabledAsync(CancellationToken cancellationToken);
    Task<bool> WriteFileAsync(string relativeDirectoryPath, string fileName, byte[] content, CancellationToken cancellationToken);
}
