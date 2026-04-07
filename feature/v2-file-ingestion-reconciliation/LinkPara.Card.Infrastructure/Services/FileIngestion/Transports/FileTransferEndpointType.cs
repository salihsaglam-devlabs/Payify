using System.ComponentModel;

namespace LinkPara.Card.Infrastructure.Services.FileIngestion.Transports;

public enum FileTransferEndpointType
{
    [Description("Source endpoint of the file transfer.")]
    Source = 1,

    [Description("Target endpoint of the file transfer.")]
    Target = 2
}