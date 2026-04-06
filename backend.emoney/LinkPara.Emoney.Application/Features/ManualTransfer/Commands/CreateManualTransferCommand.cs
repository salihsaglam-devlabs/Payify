using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace LinkPara.Emoney.Application.Features.ManualTransfer.Commands;

public class CreateManualTransferCommand : IRequest
{
    public Guid ApprovalId { get; set; }
    public string CustomerWalletNumber { get; set; }
    public decimal Amount { get; set; }
    public TransactionType TransactionType { get; set; }

    public Guid TransferRequestFile { get; set; }
    public Guid? TransferApprovalFile { get; set; }
    public string Description { get; set; }
}

public class CreateManuelTransferToUserCommandHandler : IRequestHandler<CreateManualTransferCommand>
{
    private readonly IManualTransferService _manualTransferService;

    public CreateManuelTransferToUserCommandHandler(IManualTransferService manualTransferService)
    {
        _manualTransferService = manualTransferService;
    }

    public async Task<Unit> Handle(CreateManualTransferCommand request, CancellationToken cancellationToken)
    {
        await _manualTransferService.CreateManualTransferAsync(request, cancellationToken);

        return Unit.Value;
    }
}