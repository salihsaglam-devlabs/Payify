using System.Net;
using System.Security.Cryptography;
using System.Text;
using LinkPara.Card.Application.Commons.Models.FileIngestion;
using Microsoft.Extensions.Options;
using Renci.SshNet;
using Renci.SshNet.Common;

namespace LinkPara.Card.Infrastructure.Services.FileIngestion.RemoteFiles;

public class RemoteFileClient : IRemoteFileReader, IRemoteFileWriter
{
    private readonly FileIngestionSettings _settings;

    public RemoteFileClient(IOptions<FileIngestionSettings> options)
    {
        _settings = options.Value;
    }
    public async Task<IReadOnlyCollection<string>> ListFilesAsync(string incomingPath, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_settings.Ftp.Source.Host) || string.IsNullOrWhiteSpace(incomingPath))
        {
            return [];
        }

        return ResolveSourceProtocol() == TransferProtocol.Sftp
            ? await ListFilesFromSftpAsync(incomingPath, cancellationToken)
            : await ListFilesFromFtpAsync(incomingPath, cancellationToken);
    }

    public async Task<string> ReadFileTextAsync(string incomingPath, string fileName, Encoding encoding, CancellationToken cancellationToken)
    {
        return ResolveSourceProtocol() == TransferProtocol.Sftp
            ? await GetFileTextFromSftpAsync(incomingPath, fileName, encoding, cancellationToken)
            : await GetFileTextFromFtpAsync(incomingPath, fileName, encoding, cancellationToken);
    }
    public Task<bool> IsEnabledAsync(CancellationToken cancellationToken)
    {
        _ = cancellationToken;
        var target = _settings.Ftp.Target;
        if (target?.Enabled != true)
        {
            return Task.FromResult(false);
        }

        return Task.FromResult(!string.IsNullOrWhiteSpace(target.Host) && !string.IsNullOrWhiteSpace(target.RootPath));
    }

    public Task<bool> WriteFileAsync(string relativeDirectoryPath, string fileName, byte[] content, CancellationToken cancellationToken)
    {
        var target = _settings.Ftp.Target;
        if (target?.Enabled != true
            || string.IsNullOrWhiteSpace(target.Host)
            || string.IsNullOrWhiteSpace(target.RootPath)
            || string.IsNullOrWhiteSpace(fileName))
        {
            return Task.FromResult(false);
        }

        return ResolveTargetProtocol(target) == TransferProtocol.Sftp
            ? UploadArchiveToSftpAsync(target, relativeDirectoryPath, fileName, content, cancellationToken)
            : UploadArchiveToFtpAsync(target, relativeDirectoryPath, fileName, content, cancellationToken);
    }
    private TransferProtocol ResolveSourceProtocol()
    {
        var source = _settings.Ftp.Source;
        return ResolveProtocol(source.Protocol, source.Host, source.Port);
    }

    private static TransferProtocol ResolveTargetProtocol(RemoteTargetSettings target)
    {
        return ResolveProtocol(target.Protocol, target.Host, target.Port);
    }

    private static TransferProtocol ResolveProtocol(string protocol, string host, int port)
    {
        var normalizedProtocol = (protocol ?? string.Empty).Trim();
        if (normalizedProtocol.Equals("sftp", StringComparison.OrdinalIgnoreCase))
        {
            return TransferProtocol.Sftp;
        }

        if (normalizedProtocol.Equals("ftp", StringComparison.OrdinalIgnoreCase))
        {
            return TransferProtocol.Ftp;
        }

        var normalizedHost = (host ?? string.Empty).Trim();
        if (normalizedHost.StartsWith("sftp://", StringComparison.OrdinalIgnoreCase) || port == 22)
        {
            return TransferProtocol.Sftp;
        }

        return TransferProtocol.Ftp;
    }
    private async Task<IReadOnlyCollection<string>> ListFilesFromFtpAsync(string incomingPath, CancellationToken cancellationToken)
    {
        var request = CreateFtpRequest(_settings.Ftp.Source, incomingPath, string.Empty, WebRequestMethods.Ftp.ListDirectory);
        using var response = (FtpWebResponse)await request.GetResponseAsync();
        await using var stream = response.GetResponseStream();
        using var reader = new StreamReader(stream!, Encoding.UTF8);

        var names = new List<string>();
        while (!reader.EndOfStream)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var line = await reader.ReadLineAsync(cancellationToken);
            if (!string.IsNullOrWhiteSpace(line))
            {
                names.Add(line.Trim());
            }
        }

        return names
            .Where(IsSupportedInputFile)
            .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private async Task<string> GetFileTextFromFtpAsync(string incomingPath, string fileName, Encoding encoding, CancellationToken cancellationToken)
    {
        var request = CreateFtpRequest(_settings.Ftp.Source, incomingPath, fileName, WebRequestMethods.Ftp.DownloadFile);
        using var response = (FtpWebResponse)await request.GetResponseAsync();
        await using var stream = response.GetResponseStream();
        await using var buffer = new MemoryStream();
        await stream!.CopyToAsync(buffer, cancellationToken);
        return (encoding ?? Encoding.UTF8).GetString(buffer.ToArray());
    }
    private Task<IReadOnlyCollection<string>> ListFilesFromSftpAsync(string incomingPath, CancellationToken cancellationToken)
    {
        return RunWithSftpAsync(client =>
        {
            var names = client.ListDirectory(NormalizeRemotePath(incomingPath))
                .Where(x => !x.IsDirectory && !x.IsSymbolicLink && !x.IsSocket && !x.IsBlockDevice && !x.IsCharacterDevice)
                .Select(x => x.Name)
                .Where(IsSupportedInputFile)
                .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            return (IReadOnlyCollection<string>)names;
        }, CreateSourceSftpClient, cancellationToken);
    }

    private async Task<string> GetFileTextFromSftpAsync(string incomingPath, string fileName, Encoding encoding, CancellationToken cancellationToken)
    {
        var bytes = await RunWithSftpAsync(client =>
        {
            var path = CombineRemotePath(incomingPath, fileName);
            using var buffer = new MemoryStream();
            client.DownloadFile(path, buffer);
            return buffer.ToArray();
        }, CreateSourceSftpClient, cancellationToken);

        return (encoding ?? Encoding.UTF8).GetString(bytes);
    }
    private async Task<bool> UploadArchiveToFtpAsync(
        RemoteTargetSettings target,
        string relativeDirectoryPath,
        string fileName,
        byte[] content,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var rootPath = NormalizeRemotePath(target.RootPath);
        var relativePath = NormalizeRemotePath(relativeDirectoryPath);
        var targetDirectory = string.IsNullOrWhiteSpace(relativePath)
            ? rootPath
            : $"{rootPath.TrimEnd('/')}/{relativePath.TrimStart('/')}";

        await EnsureFtpDirectoryExistsAsync(target, targetDirectory, cancellationToken);

        var request = CreateFtpRequest(target, targetDirectory, fileName, WebRequestMethods.Ftp.UploadFile);
        await using (var requestStream = await request.GetRequestStreamAsync())
        {
            var safeContent = content ?? [];
            await requestStream.WriteAsync(safeContent, 0, safeContent.Length, cancellationToken);
        }

        using var response = (FtpWebResponse)await request.GetResponseAsync();
        return response.StatusCode is FtpStatusCode.ClosingData or FtpStatusCode.FileActionOK;
    }

    private async Task EnsureFtpDirectoryExistsAsync(RemoteTargetSettings target, string fullDirectoryPath, CancellationToken cancellationToken)
    {
        var normalized = NormalizeRemotePath(fullDirectoryPath);
        var segments = normalized.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries);
        var current = string.Empty;

        foreach (var segment in segments)
        {
            cancellationToken.ThrowIfCancellationRequested();
            current = string.IsNullOrEmpty(current) ? $"/{segment}" : $"{current}/{segment}";

            try
            {
                var request = CreateFtpRequest(target, current, string.Empty, WebRequestMethods.Ftp.MakeDirectory);
                using var response = (FtpWebResponse)await request.GetResponseAsync();
                _ = response.StatusCode;
            }
            catch (WebException ex)
            {
                if (ex.Response is not FtpWebResponse ftpResponse)
                {
                    throw;
                }

                if (ftpResponse.StatusCode != FtpStatusCode.ActionNotTakenFileUnavailable)
                {
                    throw;
                }
            }
        }
    }
    private async Task<bool> UploadArchiveToSftpAsync(
        RemoteTargetSettings target,
        string relativeDirectoryPath,
        string fileName,
        byte[] content,
        CancellationToken cancellationToken)
    {
        return await Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();

            using var client = CreateTargetSftpClient(target);
            client.Connect();
            try
            {
                var rootPath = NormalizeRemotePath(target.RootPath);
                var relativePath = NormalizeRemotePath(relativeDirectoryPath);
                var targetDirectory = string.IsNullOrWhiteSpace(relativePath)
                    ? rootPath
                    : $"{rootPath.TrimEnd('/')}/{relativePath.TrimStart('/')}";

                EnsureSftpDirectoryExists(client, targetDirectory);

                var targetFilePath = $"{targetDirectory.TrimEnd('/')}/{fileName}";
                if (client.Exists(targetFilePath))
                {
                    client.DeleteFile(targetFilePath);
                }

                using var stream = new MemoryStream(content ?? []);
                client.UploadFile(stream, targetFilePath, true);
                return true;
            }
            finally
            {
                if (client.IsConnected)
                {
                    client.Disconnect();
                }
            }
        }, cancellationToken);
    }
    private bool IsSupportedInputFile(string fileName)
    {
        var ext = Path.GetExtension(fileName);
        return _settings.FileDetection.SupportedExtensions.Any(x => x.Equals(ext, StringComparison.OrdinalIgnoreCase));
    }

    private static FtpWebRequest CreateFtpRequest(RemoteEndpointSettings endpoint, string path, string fileName, string method)
    {
        var host = (endpoint.Host ?? string.Empty).Trim();
        if (!host.Contains("://", StringComparison.Ordinal))
        {
            host = $"ftp://{host}";
        }

        var builder = new UriBuilder(host)
        {
            Port = Math.Max(1, endpoint.Port)
        };

        var normalizedPath = NormalizeRemotePath(path).Trim('/');
        builder.Path = string.IsNullOrWhiteSpace(fileName)
            ? normalizedPath
            : $"{normalizedPath}/{fileName.TrimStart('/')}";

        var request = (FtpWebRequest)WebRequest.Create(builder.Uri);
        request.Method = method;
        request.Credentials = new NetworkCredential(endpoint.Username, endpoint.Password);
        request.UsePassive = endpoint.UsePassive;
        request.UseBinary = true;
        request.KeepAlive = false;
        request.Timeout = Math.Max(1, endpoint.TimeoutSeconds) * 1000;
        request.ReadWriteTimeout = Math.Max(1, endpoint.TimeoutSeconds) * 1000;
        return request;
    }

    private Task<T> RunWithSftpAsync<T>(Func<SftpClient, T> action, Func<SftpClient> clientFactory, CancellationToken cancellationToken)
    {
        return Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();

            using var client = clientFactory();
            client.Connect();
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                return action(client);
            }
            finally
            {
                if (client.IsConnected)
                {
                    client.Disconnect();
                }
            }
        }, cancellationToken);
    }

    private SftpClient CreateSourceSftpClient()
    {
        return CreateSftpClient(
            _settings.Ftp.Source,
            "SFTP username is required.",
            "SFTP authentication requires Password or PrivateKeyPath.");
    }

    private SftpClient CreateTargetSftpClient(RemoteTargetSettings target)
    {
        return CreateSftpClient(
            target,
            "Archive SFTP username is required.",
            "Archive SFTP authentication requires Password or PrivateKeyPath.");
    }

    private static SftpClient CreateSftpClient(RemoteEndpointSettings endpoint, string missingUsernameError, string missingAuthError)
    {
        var host = NormalizeHost(endpoint.Host);
        var username = endpoint.Username ?? string.Empty;
        var port = endpoint.Port > 0 ? endpoint.Port : 22;
        var timeout = TimeSpan.FromSeconds(Math.Max(1, endpoint.TimeoutSeconds));

        if (string.IsNullOrWhiteSpace(username))
        {
            throw new InvalidOperationException(missingUsernameError);
        }

        var authMethods = BuildAuthMethods(username, endpoint.Password, endpoint.PrivateKeyPath, endpoint.PrivateKeyPassphrase, missingAuthError);

        var connectionInfo = new ConnectionInfo(host, port, username, authMethods)
        {
            Timeout = timeout
        };

        var client = new SftpClient(connectionInfo)
        {
            KeepAliveInterval = timeout,
            OperationTimeout = timeout
        };

        if (!string.IsNullOrWhiteSpace(endpoint.KnownHostFingerprint))
        {
            client.HostKeyReceived += (_, e) =>
            {
                e.CanTrust = IsFingerprintMatch(endpoint.KnownHostFingerprint, e);
            };
        }

        return client;
    }

    private static AuthenticationMethod[] BuildAuthMethods(
        string username,
        string password,
        string privateKeyPath,
        string privateKeyPassphrase,
        string noAuthError)
    {
        var methods = new List<AuthenticationMethod>();

        if (!string.IsNullOrWhiteSpace(password))
        {
            methods.Add(new PasswordAuthenticationMethod(username, password));
        }

        if (!string.IsNullOrWhiteSpace(privateKeyPath))
        {
            var keyFile = string.IsNullOrWhiteSpace(privateKeyPassphrase)
                ? new PrivateKeyFile(privateKeyPath)
                : new PrivateKeyFile(privateKeyPath, privateKeyPassphrase);
            methods.Add(new PrivateKeyAuthenticationMethod(username, keyFile));
        }

        if (methods.Count == 0)
        {
            throw new InvalidOperationException(noAuthError);
        }

        return methods.ToArray();
    }

    private static void EnsureSftpDirectoryExists(SftpClient client, string path)
    {
        var normalized = NormalizeRemotePath(path);
        if (normalized == "/")
        {
            return;
        }

        var segments = normalized.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries);
        var current = "/";

        foreach (var segment in segments)
        {
            current = current.EndsWith("/", StringComparison.Ordinal)
                ? $"{current}{segment}"
                : $"{current}/{segment}";

            if (!client.Exists(current))
            {
                client.CreateDirectory(current);
            }
        }
    }

    private static bool IsFingerprintMatch(string expected, HostKeyEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(expected))
        {
            return true;
        }

        var trimmed = expected.Trim();
        if (trimmed.StartsWith("SHA256:", StringComparison.OrdinalIgnoreCase))
        {
            var expectedSha256 = trimmed["SHA256:".Length..].Trim();
            var actualSha256 = Convert.ToBase64String(SHA256.HashData(e.HostKey ?? []));
            return string.Equals(expectedSha256.TrimEnd('='), actualSha256.TrimEnd('='), StringComparison.Ordinal);
        }

        var expectedHex = NormalizeHexFingerprint(trimmed);
        var actualHex = NormalizeHexFingerprint(Convert.ToHexString(e.FingerPrint ?? []));
        return string.Equals(expectedHex, actualHex, StringComparison.Ordinal);
    }

    private static string NormalizeHexFingerprint(string fingerprint)
    {
        return (fingerprint ?? string.Empty)
            .Replace(":", string.Empty, StringComparison.Ordinal)
            .Replace("-", string.Empty, StringComparison.Ordinal)
            .Trim()
            .ToUpperInvariant();
    }

    private static string NormalizeHost(string host)
    {
        var value = (host ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        if (value.Contains("://", StringComparison.Ordinal)
            && Uri.TryCreate(value, UriKind.Absolute, out var uri))
        {
            return uri.Host;
        }

        var firstSlash = value.IndexOf('/');
        return firstSlash > -1 ? value[..firstSlash] : value;
    }

    private static string NormalizeRemotePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return "/";
        }

        return "/" + path.Trim().Trim('/');
    }

    private static string CombineRemotePath(string directoryPath, string fileName)
    {
        var dir = NormalizeRemotePath(directoryPath).TrimEnd('/');
        return $"{dir}/{(fileName ?? string.Empty).TrimStart('/')}";
    }

    private enum TransferProtocol
    {
        Ftp = 1,
        Sftp = 2
    }
}
