using LinkPara.Emoney.Application.Features.ManualTransfer.Commands;

namespace LinkPara.Emoney.Application.Commons.Interfaces;

public interface  IManualTransferService
{
    Task CreateManualTransferAsync(CreateManualTransferCommand request, CancellationToken cancellationToken);
}