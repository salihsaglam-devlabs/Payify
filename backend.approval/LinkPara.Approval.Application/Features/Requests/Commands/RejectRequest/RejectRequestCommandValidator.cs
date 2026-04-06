using FluentValidation;

namespace LinkPara.Approval.Application.Features.Requests.Commands.RejectRequest;

public class RejectRequestCommandValidator : AbstractValidator<RejectRequestCommand>
{
    public RejectRequestCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotNull()
            .NotEmpty();
        RuleFor(x => x.RequestId)
            .NotNull()
            .NotEmpty();
        RuleFor(x => x.CheckerRoleIds)
            .NotNull()
            .NotEmpty();
        RuleFor(x => x.CheckerFullName)
            .NotNull()
            .NotEmpty()
            .MaximumLength(100);
        RuleFor(x => x.Reason)
            .NotNull()
            .NotEmpty()
            .MaximumLength(1500);
    }
}
