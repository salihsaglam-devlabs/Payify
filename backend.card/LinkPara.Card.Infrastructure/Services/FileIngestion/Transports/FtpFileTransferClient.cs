using LinkPara.Card.Application.Commons.Models.FileIngestion;
using Microsoft.Extensions.Localization;
using LinkPara.Card.Domain.Enums.FileIngestion;
using Microsoft.Extensions.Options;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace LinkPara.Card.Infrastructure.Services.FileIngestion.Transports;

public class FtpFileTransferClient : IFileTransferClient
{
    private readonly FileIngestionOptions _options;
    private readonly IStringLocalizer _localizer;

    public FtpFileTransferClient(IOptions<FileIngestionOptions> options, Func<LinkPara.Card.Application.Commons.Localization.LocalizerResource, IStringLocalizer> localizerFactory)
    {
        _options = options.Value;
        _localizer = localizerFactory(LinkPara.Card.Application.Commons.Localization.LocalizerResource.Messages);
    }

    public FileSourceType SourceType => FileSourceType.Remote;

    public async Task<IReadOnlyCollection<FileReference>> ListAsync(
        string profileKey,
        ProfileOptions profile,
        FileTransferEndpointType endpointType = FileTransferEndpointType.Source,
        CancellationToken cancellationToken = default)
    {
        var ftpOptions = GetFtpOptions(endpointType);
        if (!ftpOptions.Paths.TryGetValue(profileKey, out var remotePath))
            return Array.Empty<FileReference>();

        var regex = new Regex(profile.Pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
        var request = CreateRequest(remotePath, WebRequestMethods.Ftp.ListDirectory, endpointType);
        using var response = (FtpWebResponse)await request.GetResponseAsync();
        await using var stream = response.GetResponseStream();
        using var reader = new StreamReader(stream ?? Stream.Null);

        var files = new List<FileReference>();
        while (!reader.EndOfStream)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var line = await reader.ReadLineAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(line))
                continue;

            if (!profile.FileExtensions.Contains(Path.GetExtension(line), StringComparer.OrdinalIgnoreCase))
                continue;

            if (!regex.IsMatch(Path.GetFileNameWithoutExtension(line)))
                continue;

            files.Add(new FileReference
            {
                Name = line.Trim(),
                FullPath = $"{remotePath.TrimEnd('/')}/{line.Trim()}"
            });
        }

        return files
            .OrderBy(x => x.Name, FileReferenceNameComparer.Instance)
            .ToArray();
    }

    public async Task<Stream> OpenReadAsync(FileReference file, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var request = CreateRequest(file.FullPath, WebRequestMethods.Ftp.DownloadFile, FileTransferEndpointType.Source);
        var response = (FtpWebResponse)await request.GetResponseAsync();
        var stream = response.GetResponseStream() ?? Stream.Null;
        return new FtpResponseStream(stream, response);
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
            await SkipBytesAsync(stream, byteOffset, cancellationToken);

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
        cancellationToken.ThrowIfCancellationRequested();
        var resolvedFileName = ResolveTargetFileName(fileName, endpointType);
        var remotePath = BuildRemoteFilePath(profileKey, resolvedFileName, endpointType);
        var request = CreateRequest(remotePath, WebRequestMethods.Ftp.UploadFile, endpointType);
        var requestStream = await request.GetRequestStreamAsync();
        return new FtpRequestStream(requestStream, request);
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
        var request = CreateRequest(remotePath, WebRequestMethods.Ftp.UploadFile, endpointType);
        await using var stream = new FtpRequestStream(await request.GetRequestStreamAsync(), request);
        await stream.WriteAsync(content, cancellationToken);
        await stream.FlushAsync(cancellationToken);
        return new FileReference
        {
            Name = resolvedFileName,
            FullPath = remotePath
        };
    }

    private FtpWebRequest CreateRequest(string remotePath, string method, FileTransferEndpointType endpointType)
    {
        var ftpOptions = GetFtpOptions(endpointType);
        var request = (FtpWebRequest)WebRequest.Create(new Uri($"ftp://{ftpOptions.Host}:{ftpOptions.Port.Value}{NormalizeRemotePath(remotePath)}"));
        request.Method = method;
        request.Credentials = new NetworkCredential(ftpOptions.Username, ftpOptions.Password);
        request.UseBinary = true;
        request.UsePassive = ftpOptions.UsePassive.Value;
        request.Timeout = ftpOptions.TimeoutSeconds.Value * 1000;
        request.ReadWriteTimeout = ftpOptions.TimeoutSeconds.Value * 1000;
        return request;
    }

    private FtpOptions GetFtpOptions(FileTransferEndpointType endpointType)
    {
        return endpointType == FileTransferEndpointType.Target
            ? _options.Connections.Target.Ftp
            : _options.Connections.Source.Ftp;
    }

    private string BuildRemoteFilePath(string profileKey, string fileName, FileTransferEndpointType endpointType)
    {
        var ftpOptions = GetFtpOptions(endpointType);
        if (!ftpOptions.Paths.TryGetValue(profileKey, out var remotePath))
            throw new InvalidOperationException(_localizer.Get("FileIngestion.FtpPathNotConfigured", profileKey));

        return $"{remotePath.TrimEnd('/')}/{fileName}";
    }

    private static string NormalizeRemotePath(string remotePath) => remotePath.StartsWith('/') ? remotePath : $"/{remotePath}";

    private static string ResolveTargetFileName(string fileName, FileTransferEndpointType endpointType)
        => endpointType == FileTransferEndpointType.Target
            ? AppendTimestampToFileName(fileName)
            : fileName;

    private static string AppendTimestampToFileName(string fileName)
    {
        var extension = Path.GetExtension(fileName);
        var nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
        return $"{nameWithoutExtension}_{DateTime.Now:yyyyMMddHHmmssfff}{extension}";
    }

    private static async Task SkipBytesAsync(Stream stream, long byteOffset, CancellationToken cancellationToken)
    {
        var buffer = new byte[8192];
        var remaining = byteOffset;
        while (remaining > 0)
        {
            var read = await stream.ReadAsync(buffer.AsMemory(0, (int)Math.Min(buffer.Length, remaining)), cancellationToken);
            if (read == 0)
                break;

            remaining -= read;
        }
    }

    private sealed class FtpResponseStream(Stream innerStream, FtpWebResponse response) : Stream
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
            response.Dispose();
            base.Dispose(disposing);
        }

        private async ValueTask DisposeAsyncCore()
        {
            await innerStream.DisposeAsync();
            response.Dispose();
            await base.DisposeAsync();
        }
    }

    private sealed class FtpRequestStream(Stream innerStream, FtpWebRequest request) : Stream
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
            using var response = (FtpWebResponse)request.GetResponse();
            base.Dispose(disposing);
        }

        private async ValueTask DisposeAsyncCore()
        {
            await innerStream.DisposeAsync();
            using var response = (FtpWebResponse)await request.GetResponseAsync();
            await base.DisposeAsync();
        }
    }
}
