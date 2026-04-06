using FluentValidation;

namespace LinkPara.Emoney.Application.Features.Topups.Commands.TopupUpdateStatus;

public class TopupUpdateStatusCommandValidator : AbstractValidator<TopupUpdateStatusCommand>
{
    public TopupUpdateStatusCommandValidator()
    {
        RuleFor(r => r.CardTopupRequestId)
        .NotNull()
        .NotEmpty();

        RuleFor(r => r.Status)
        .NotNull()
        .NotEmpty();
    }
}
