using FluentValidation;

namespace LinkPara.Approval.Application.Features.Requests.Commands.SaveRequest;

public class SaveRequestCommandValidator : AbstractValidator<SaveRequestCommand>
{
    public SaveRequestCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotNull()
            .NotEmpty();
        RuleFor(x => x.MakerRoleIds)
            .NotNull()
            .NotEmpty();
        RuleFor(x => x.CaseId)
            .NotNull()
            .NotEmpty(); 
        RuleFor(x => x.Resource)
            .NotNull()
            .NotEmpty();
        RuleFor(x => x.Url)
            .NotNull()
            .NotEmpty();
        RuleFor(x => x.DisplayName)
            .NotNull()
            .NotEmpty();
        RuleFor(x => x.MakerFullName)
            .NotNull()
            .NotEmpty();
    }
}