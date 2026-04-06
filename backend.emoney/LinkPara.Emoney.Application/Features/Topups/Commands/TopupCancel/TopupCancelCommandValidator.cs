using FluentValidation;

namespace LinkPara.Emoney.Application.Features.Topups.Commands.TopupCancel;

public class TopupCancelCommandValidator : AbstractValidator<TopupCancelCommand>
{
    public TopupCancelCommandValidator()
    {
        RuleFor(r => r.BaseRequest.CardTopupRequestId)
        .NotNull()
        .NotEmpty();

        RuleFor(r => r.BaseRequest.Description)
        .NotNull()
        .NotEmpty();
    }
}
