using LinkPara.Card.Application.Commons.Models.FileIngestion;
using LinkPara.Card.Domain.Enums.FileIngestion;
using Microsoft.Extensions.Options;
using Renci.SshNet;
using Renci.SshNet.Common;
using System.Text;
using System.Text.RegularExpressions;

namespace LinkPara.Card.Infrastructure.Services.FileIngestion.Transports;

public class SftpFileTransferClient : IFileTransferClient
{
    private readonly FileIngestionOptions _options = new();

    public SftpFileTransferClient(IOptions<FileIngestionOptions> options)
    {
        _options = options.Value;
    }

    public FileSourceType SourceType => FileSourceType.Remote;

    public Task<IReadOnlyCollection<FileReference>> ListAsync(
        string profileKey,
        ProfileOptions profile,
        FileTransferEndpointType endpointType = FileTransferEndpointType.Source,
        CancellationToken cancellationToken = default)
    {
        return Task.Run<IReadOnlyCollection<FileReference>>(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            using var client = CreateClient(endpointType);
            client.Connect();

            var sftpOptions = GetSftpOptions(endpointType);
            if (!sftpOptions.Paths.TryGetValue(profileKey, out var remotePath))
                return Array.Empty<FileReference>();

            var regex = new Regex(profile.Pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            var files = client.ListDirectory(remotePath)
                .Where(x => !x.IsDirectory && !x.IsSymbolicLink)
                .Where(x => profile.FileExtensions.Contains(Path.GetExtension(x.Name), StringComparer.OrdinalIgnoreCase))
                .Where(x => regex.IsMatch(Path.GetFileNameWithoutExtension(x.Name)))
                .OrderBy(x => x.Name, FileReferenceNameComparer.Instance)
                .Select(x => new FileReference
                {
                    Name = x.Name,
                    FullPath = x.FullName
                })
                .ToArray();

            client.Disconnect();
            return files;
        }, cancellationToken);
    }

    public Task<Stream> OpenReadAsync(FileReference file, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var client = CreateClient(FileTransferEndpointType.Source);
        client.Connect();
        var stream = client.OpenRead(file.FullPath);
        return Task.FromResult<Stream>(new SftpClientStream(stream, client));
    }

    public async Task<byte[]> ReadAllBytesAsync(FileReference file, CancellationToken cancellationToken = default)
    {
        await using var stream = await OpenReadAsync(file, cancellationToken);
        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream, cancellationToken);
        return memoryStream.ToArray();
    }

    public async Task<string> ReadRangeAsync(
        FileReference file,
        long byteOffset,
        int byteLength,
        Encoding encoding,
        CancellationToken cancellationToken = default)
    {
        await using var stream = await OpenReadAsync(file, cancellationToken);
        if (byteOffset > 0)
            stream.Seek(byteOffset, SeekOrigin.Begin);

        var buffer = new byte[byteLength];
        var totalRead = 0;
        while (totalRead < byteLength)
        {
            var read = await stream.ReadAsync(buffer.AsMemory(totalRead, byteLength - totalRead), cancellationToken);
            if (read == 0)
                break;

            totalRead += read;
        }

        return encoding.GetString(buffer, 0, totalRead);
    }

    public Task<Stream> OpenWriteAsync(
        string profileKey,
        string fileName,
        FileTransferEndpointType endpointType = FileTransferEndpointType.Target,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var client = CreateClient(endpointType);
        client.Connect();
        var resolvedFileName = ResolveTargetFileName(fileName, endpointType);
        var remotePath = BuildRemoteFilePath(profileKey, resolvedFileName, endpointType);
        EnsureDirectoryExists(client, Path.GetDirectoryName(remotePath)!.Replace('\\', '/'));
        var stream = client.OpenWrite(remotePath);
        return Task.FromResult<Stream>(new SftpClientStream(stream, client));
    }

    public async Task<FileReference> WriteAllBytesAsync(
        string profileKey,
        string fileName,
        ReadOnlyMemory<byte> content,
        FileTransferEndpointType endpointType = FileTransferEndpointType.Target,
        CancellationToken cancellationToken = default)
    {
        var resolvedFileName = ResolveTargetFileName(fileName, endpointType);
        var remotePath = BuildRemoteFilePath(profileKey, resolvedFileName, endpointType);
        using var client = CreateClient(endpointType);
        client.Connect();
        EnsureDirectoryExists(client, Path.GetDirectoryName(remotePath)!.Replace('\\', '/'));
        await using var stream = new SftpClientStream(client.OpenWrite(remotePath), client);
        await stream.WriteAsync(content, cancellationToken);
        await stream.FlushAsync(cancellationToken);
        return new FileReference
        {
            Name = resolvedFileName,
            FullPath = remotePath
        };
    }

    private SftpOptions GetSftpOptions(FileTransferEndpointType endpointType)
    {
        return endpointType == FileTransferEndpointType.Target
            ? _options.Connections.Target.Sftp
            : _options.Connections.Source.Sftp;
    }

    private string BuildRemoteFilePath(string profileKey, string fileName, FileTransferEndpointType endpointType)
    {
        var sftpOptions = GetSftpOptions(endpointType);
        if (!sftpOptions.Paths.TryGetValue(profileKey, out var remotePath))
            throw new InvalidOperationException($"SFTP path is not configured for profile '{profileKey}'.");

        return $"{remotePath.TrimEnd('/')}/{fileName}";
    }

    private SftpClient CreateClient(FileTransferEndpointType endpointType)
    {
        var sftpOptions = GetSftpOptions(endpointType);
        var authenticationMethods = new List<AuthenticationMethod>();

        if (!string.IsNullOrWhiteSpace(sftpOptions.PrivateKeyPath))
        {
            var keyFile = string.IsNullOrWhiteSpace(sftpOptions.PrivateKeyPassphrase)
                ? new PrivateKeyFile(sftpOptions.PrivateKeyPath)
                : new PrivateKeyFile(sftpOptions.PrivateKeyPath, sftpOptions.PrivateKeyPassphrase);
            authenticationMethods.Add(new PrivateKeyAuthenticationMethod(sftpOptions.Username, keyFile));
        }

        if (!string.IsNullOrWhiteSpace(sftpOptions.Password))
            authenticationMethods.Add(new PasswordAuthenticationMethod(sftpOptions.Username, sftpOptions.Password));

        var connectionInfo = new ConnectionInfo(
            sftpOptions.Host,
            sftpOptions.Port,
            sftpOptions.Username,
            authenticationMethods.ToArray());
        var client = new SftpClient(connectionInfo);

        if (!string.IsNullOrWhiteSpace(sftpOptions.KnownHostFingerprint))
        {
            client.HostKeyReceived += (_, e) =>
            {
                var fingerprint = Convert.ToHexString(e.FingerPrint).ToLowerInvariant();
                var expected = sftpOptions.KnownHostFingerprint
                    .Replace(":", string.Empty, StringComparison.Ordinal)
                    .ToLowerInvariant();
                e.CanTrust = fingerprint == expected;
            };
        }

        return client;
    }

    private static void EnsureDirectoryExists(SftpClient client, string remoteDirectory)
    {
        if (string.IsNullOrWhiteSpace(remoteDirectory) || remoteDirectory == "/")
            return;

        var parts = remoteDirectory.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var current = string.Empty;
        foreach (var part in parts)
        {
            current += "/" + part;
            if (!client.Exists(current))
                client.CreateDirectory(current);
        }
    }

    private static string AppendTimestampToFileName(string fileName)
    {
        var extension = Path.GetExtension(fileName);
        var nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
        return $"{nameWithoutExtension}_{DateTime.Now:yyyyMMddHHmmssfff}{extension}";
    }

    private static string ResolveTargetFileName(string fileName, FileTransferEndpointType endpointType)
        => endpointType == FileTransferEndpointType.Target
            ? AppendTimestampToFileName(fileName)
            : fileName;

    private sealed class SftpClientStream(Stream innerStream, SftpClient client) : Stream
    {
        public override bool CanRead => innerStream.CanRead;
        public override bool CanSeek => innerStream.CanSeek;
        public override bool CanWrite => innerStream.CanWrite;
        public override long Length => innerStream.Length;
        public override long Position { get => innerStream.Position; set => innerStream.Position = value; }
        public override void Flush() => innerStream.Flush();
        public override int Read(byte[] buffer, int offset, int count) => innerStream.Read(buffer, offset, count);
        public override long Seek(long offset, SeekOrigin origin) => innerStream.Seek(offset, origin);
        public override void SetLength(long value) => innerStream.SetLength(value);
        public override void Write(byte[] buffer, int offset, int count) => innerStream.Write(buffer, offset, count);
        public override ValueTask DisposeAsync() => DisposeAsyncCore();

        protected override void Dispose(bool disposing)
        {
            if (!disposing)
                return;

            innerStream.Dispose();
            if (client.IsConnected)
                client.Disconnect();
            client.Dispose();
            base.Dispose(disposing);
        }

        private async ValueTask DisposeAsyncCore()
        {
            await innerStream.DisposeAsync();
            if (client.IsConnected)
                client.Disconnect();
            client.Dispose();
            await base.DisposeAsync();
        }
    }
}
