using LinkPara.Card.Domain.Enums.FileIngestion;

namespace LinkPara.Card.Infrastructure.Services.FileIngestion.Transports;

public interface IFileTransferClientResolver
{
    IFileTransferClient Create(
        FileSourceType sourceType,
        FileTransferEndpointType endpointType = FileTransferEndpointType.Source);
}
