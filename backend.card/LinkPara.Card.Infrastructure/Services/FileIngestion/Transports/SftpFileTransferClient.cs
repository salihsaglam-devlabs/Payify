using LinkPara.Card.Application.Commons.Exceptions;
using LinkPara.Card.Application.Commons.Interfaces;
using Microsoft.Extensions.Localization;
using LinkPara.Card.Domain.Enums.FileIngestion;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Renci.SshNet;
using Renci.SshNet.Common;
using System.Text;
using System.Text.RegularExpressions;
using LinkPara.Card.Application.Commons.Extensions;
using LinkPara.Card.Application.Commons.Models.FileIngestion.Configuration;

namespace LinkPara.Card.Infrastructure.Services.FileIngestion.Transports;

public class SftpFileTransferClient : IFileTransferClient
{
    private readonly FileIngestionOptions _options;
    private readonly IStringLocalizer _localizer;
    private readonly ITimeProvider _timeProvider;
    private readonly ILogger<SftpFileTransferClient> _logger;

    public SftpFileTransferClient(
        IOptions<FileIngestionOptions> options,
        ITimeProvider timeProvider,
        ILogger<SftpFileTransferClient> logger,
        Func<LinkPara.Card.Application.Commons.Localization.LocalizerResource, IStringLocalizer> localizerFactory)
    {
        _options = options.Value;
        _timeProvider = timeProvider;
        _logger = logger;
        _localizer = localizerFactory(LinkPara.Card.Application.Commons.Localization.LocalizerResource.Messages);
    }

    public FileSourceType SourceType => FileSourceType.Remote;

    public async Task<IReadOnlyCollection<FileReference>> ListAsync(
        string profileKey,
        ProfileOptions profile,
        FileTransferEndpointType endpointType = FileTransferEndpointType.Source,
        CancellationToken cancellationToken = default)
    {
        using var client = CreateClient(endpointType);
        await ConnectWithRetryAsync(client, endpointType, cancellationToken);

        var sftpOptions = GetSftpOptions(endpointType);
        if (!sftpOptions.Paths.TryGetValue(profileKey, out var remotePath))
            return Array.Empty<FileReference>();

        var regex = new Regex(profile.Pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);

        var files = await Task.Run(() =>
            client.ListDirectory(remotePath)
                .Where(x => !x.IsDirectory && !x.IsSymbolicLink)
                .Where(x => profile.FileExtensions.Contains(Path.GetExtension(x.Name), StringComparer.OrdinalIgnoreCase))
                .Where(x => regex.IsMatch(Path.GetFileNameWithoutExtension(x.Name)))
                .OrderBy(x => x.Name, FileReferenceNameComparer.Instance)
                .Select(x => new FileReference
                {
                    Name = x.Name,
                    FullPath = x.FullName
                })
                .ToArray(),
            cancellationToken);

        client.Disconnect();
        return files;
    }

    public async Task<Stream> OpenReadAsync(FileReference file, CancellationToken cancellationToken = default)
    {
        var client = CreateClient(FileTransferEndpointType.Source);
        try
        {
            await ConnectWithRetryAsync(client, FileTransferEndpointType.Source, cancellationToken);
            var stream = client.OpenRead(file.FullPath);
            return new SftpClientStream(stream, client);
        }
        catch
        {
            client.Dispose();
            throw;
        }
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

    public async Task<Stream> OpenWriteAsync(
        string profileKey,
        string fileName,
        FileTransferEndpointType endpointType = FileTransferEndpointType.Target,
        CancellationToken cancellationToken = default)
    {
        var client = CreateClient(endpointType);
        try
        {
            await ConnectWithRetryAsync(client, endpointType, cancellationToken);
            var resolvedFileName = ResolveTargetFileName(fileName, endpointType);
            var remotePath = BuildRemoteFilePath(profileKey, resolvedFileName, endpointType);
            EnsureDirectoryExists(client, Path.GetDirectoryName(remotePath)!.Replace('\\', '/'));
            var stream = client.OpenWrite(remotePath);
            return new SftpClientStream(stream, client);
        }
        catch
        {
            client.Dispose();
            throw;
        }
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
        await ConnectWithRetryAsync(client, endpointType, cancellationToken);
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

    private async Task ConnectWithRetryAsync(
        SftpClient client,
        FileTransferEndpointType endpointType,
        CancellationToken cancellationToken)
    {
        var sftpOptions = GetSftpOptions(endpointType);
        var maxRetries = sftpOptions.RetryCount ?? SftpOptions.DefaultRetryCount;
        var retryDelay = sftpOptions.RetryDelaySeconds ?? SftpOptions.DefaultRetryDelaySeconds;
        var host = sftpOptions.Host;
        var port = sftpOptions.Port ?? SftpOptions.DefaultPort;

        for (var attempt = 1; attempt <= maxRetries + 1; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                _logger.LogInformation(
                    _localizer.Get("FileIngestion.SftpConnecting"),
                    host, port, attempt, maxRetries + 1, endpointType, sftpOptions.TimeoutSeconds ?? SftpOptions.DefaultTimeoutSeconds);

                await client.ConnectAsync(cancellationToken);

                _logger.LogInformation(_localizer.Get("FileIngestion.SftpConnected"), host, port, attempt);
                return;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex) when (ex is SshOperationTimeoutException or SshConnectionException or System.Net.Sockets.SocketException)
            {
                if (attempt > maxRetries)
                {
                    _logger.LogError(ex,
                        _localizer.Get("FileIngestion.SftpConnectionFailed"),
                        host, port, maxRetries + 1, ex.Message);
                    throw;
                }

                var delay = retryDelay * (int)Math.Pow(2, attempt - 1);
                _logger.LogWarning(ex,
                    _localizer.Get("FileIngestion.SftpConnectionRetry"),
                    attempt, maxRetries + 1, host, port, ex.Message, delay);

                await Task.Delay(TimeSpan.FromSeconds(delay), cancellationToken);
            }
        }
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
            throw new FileIngestionSftpPathNotConfiguredException(_localizer.Get("FileIngestion.SftpPathNotConfigured", profileKey));

        var basePath = remotePath.TrimEnd('/');

        if (endpointType == FileTransferEndpointType.Target)
        {
            var now = _timeProvider.Now;
            var dateFolder = now.ToString("yyyy-MM-dd");
            var timeFolder = now.ToString("HH-mm-ss-fff");
            return $"{basePath}/{dateFolder}/{timeFolder}/{fileName}";
        }

        return $"{basePath}/{fileName}";
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
            sftpOptions.Port ?? SftpOptions.DefaultPort,
            sftpOptions.Username,
            authenticationMethods.ToArray())
        {
            Timeout = TimeSpan.FromSeconds(sftpOptions.TimeoutSeconds ?? SftpOptions.DefaultTimeoutSeconds)
        };
        var client = new SftpClient(connectionInfo)
        {
            OperationTimeout = TimeSpan.FromSeconds(sftpOptions.OperationTimeoutSeconds ?? SftpOptions.DefaultOperationTimeoutSeconds)
        };

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

    private string AppendTimestampToFileName(string fileName)
    {
        var extension = Path.GetExtension(fileName);
        var nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
        return $"{nameWithoutExtension}_{_timeProvider.Now:yyyyMMddHHmmssfff}{extension}";
    }

    private string ResolveTargetFileName(string fileName, FileTransferEndpointType endpointType)
        => endpointType == FileTransferEndpointType.Target
            ? AppendTimestampToFileName(fileName)
            : fileName;

    private sealed class SftpClientStream(Stream innerStream, SftpClient client) : Stream
    {
        private volatile bool _disposed;

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
            if (!disposing || _disposed)
                return;

            _disposed = true;
            innerStream.Dispose();
            DisconnectAndDisposeClient();
            base.Dispose(disposing);
        }

        private async ValueTask DisposeAsyncCore()
        {
            if (_disposed)
                return;

            _disposed = true;
            await innerStream.DisposeAsync();
            DisconnectAndDisposeClient();
        }

        private void DisconnectAndDisposeClient()
        {
            try
            {
                if (client.IsConnected)
                    client.Disconnect();
            }
            catch (ObjectDisposedException)
            {
                // Client already disposed — ignore
            }

            try
            {
                client.Dispose();
            }
            catch (ObjectDisposedException)
            {
                // Already disposed — ignore
            }
        }
    }
}
