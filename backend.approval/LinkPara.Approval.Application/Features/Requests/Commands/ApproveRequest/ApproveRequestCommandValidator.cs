using FluentValidation;

namespace LinkPara.Approval.Application.Features.Requests.Commands.ApproveRequest;

public class ApproveRequestCommandValidator : AbstractValidator<ApproveRequestCommand>
{
    public ApproveRequestCommandValidator()
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
    }
}
