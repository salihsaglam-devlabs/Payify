using FluentValidation;

namespace LinkPara.Emoney.Application.Features.BulkTransfers.Commands.ApproveBulkTransfer;

public class ActionBulkTransferCommandValidator : AbstractValidator<ActionBulkTransferCommand>
{
    public ActionBulkTransferCommandValidator()
    {
        RuleFor(x => x.BulkTransferId)
            .NotNull()
            .NotEmpty();
    }
}
