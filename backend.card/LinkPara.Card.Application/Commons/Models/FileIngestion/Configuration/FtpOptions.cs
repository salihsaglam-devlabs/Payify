using LinkPara.Card.Application.Commons.Exceptions;

namespace LinkPara.Card.Application.Commons.Models.FileIngestion.Configuration;

public class FtpOptions
{
    public const int DefaultTimeoutSeconds = 300;
    public const int DefaultPort = 21;
    public const int DefaultRetryCount = 3;
    public const int DefaultRetryDelaySeconds = 10;

    public string Host { get; set; }
    public int? Port { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public bool? UsePassive { get; set; }
    public int? TimeoutSeconds { get; set; }
    public int? RetryCount { get; set; }
    public int? RetryDelaySeconds { get; set; }
    public Dictionary<string, string> Paths { get; set; }

    public void ValidateAndApplyDefaults()
    {
        Port ??= DefaultPort;
        TimeoutSeconds ??= DefaultTimeoutSeconds;
        UsePassive ??= true;
        RetryCount ??= DefaultRetryCount;
        RetryDelaySeconds ??= DefaultRetryDelaySeconds;

        if (TimeoutSeconds <= 0)
            throw new FileIngestionFtpTimeoutInvalidException($"FtpOptions.TimeoutSeconds must be positive. Current: {TimeoutSeconds}");
        if (RetryCount < 0)
            throw new FileIngestionFtpRetryCountInvalidException($"FtpOptions.RetryCount must be non-negative. Current: {RetryCount}");
        if (RetryDelaySeconds < 0)
            throw new FileIngestionFtpRetryDelayInvalidException($"FtpOptions.RetryDelaySeconds must be non-negative. Current: {RetryDelaySeconds}");
    }
}
