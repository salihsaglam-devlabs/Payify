using LinkPara.Card.Application.Commons.Exceptions;

namespace LinkPara.Card.Application.Commons.Models.FileIngestion.Configuration;

public class SftpOptions
{
    public const int DefaultTimeoutSeconds = 60;
    public const int DefaultOperationTimeoutSeconds = 120;
    public const int DefaultPort = 22;
    public const int DefaultRetryCount = 3;
    public const int DefaultRetryDelaySeconds = 5;

    public string Host { get; set; }
    public int? Port { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public string PrivateKeyPath { get; set; }
    public string PrivateKeyPassphrase { get; set; }
    public string KnownHostFingerprint { get; set; }
    public int? TimeoutSeconds { get; set; }
    public int? OperationTimeoutSeconds { get; set; }
    public int? RetryCount { get; set; }
    public int? RetryDelaySeconds { get; set; }
    public Dictionary<string, string> Paths { get; set; }

    public void ValidateAndApplyDefaults()
    {
        Port ??= DefaultPort;
        TimeoutSeconds ??= DefaultTimeoutSeconds;
        OperationTimeoutSeconds ??= DefaultOperationTimeoutSeconds;
        RetryCount ??= DefaultRetryCount;
        RetryDelaySeconds ??= DefaultRetryDelaySeconds;

        if (TimeoutSeconds <= 0)
            throw new FileIngestionSftpTimeoutInvalidException($"SftpOptions.TimeoutSeconds must be positive. Current: {TimeoutSeconds}");
        if (OperationTimeoutSeconds <= 0)
            throw new FileIngestionSftpOperationTimeoutInvalidException($"SftpOptions.OperationTimeoutSeconds must be positive. Current: {OperationTimeoutSeconds}");
        if (RetryCount < 0)
            throw new FileIngestionSftpRetryCountInvalidException($"SftpOptions.RetryCount must be non-negative. Current: {RetryCount}");
        if (RetryDelaySeconds < 0)
            throw new FileIngestionSftpRetryDelayInvalidException($"SftpOptions.RetryDelaySeconds must be non-negative. Current: {RetryDelaySeconds}");
    }
}
