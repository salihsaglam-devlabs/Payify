using FluentValidation;

namespace LinkPara.Emoney.Application.Features.BulkTransfers.Commands.SaveBulkTransfer;

public class SaveBulkTransferCommandValidator : AbstractValidator<SaveBulkTransferCommand>
{
    public SaveBulkTransferCommandValidator()
    {

        RuleFor(x => x.SenderWalletNumber)
            .NotNull()
            .NotEmpty();

        RuleForEach(x => x.BulkTransferDetails)
            .SetValidator(new BulkTransferDetailRequestValidator());
    }
}

public class BulkTransferDetailRequestValidator : AbstractValidator<BulkTransferDetailRequest>
{
    public BulkTransferDetailRequestValidator()
    {
        RuleFor(x=> x.CurrencyCode)
            .NotNull()
            .NotEmpty();
        RuleFor(x => x.FullName)
            .NotNull()
            .NotEmpty();
        RuleFor(x => x.Description)
            .NotNull()
            .NotEmpty();
        RuleFor(x => x.Receiver)
            .NotNull()
            .NotEmpty();
        RuleFor(x => x.Amount)
            .NotNull()
            .NotEmpty()
            .GreaterThan(0);

    }
}