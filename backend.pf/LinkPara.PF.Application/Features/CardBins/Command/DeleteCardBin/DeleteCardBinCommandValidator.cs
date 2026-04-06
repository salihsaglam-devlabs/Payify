using FluentValidation;

namespace LinkPara.PF.Application.Features.CardBins.Command.DeleteCardBin;

public class DeleteCardBinCommandValidator : AbstractValidator<DeleteCardBinCommand>
{
    public DeleteCardBinCommandValidator()
    {
        RuleFor(x => x.Id)
        .NotNull().NotEmpty();
    }
}
