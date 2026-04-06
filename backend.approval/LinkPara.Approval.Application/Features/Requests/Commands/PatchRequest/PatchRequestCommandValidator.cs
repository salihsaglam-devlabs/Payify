
using FluentValidation;

namespace LinkPara.Approval.Application.Features.Requests.Commands.PatchRequest;

public class PatchRequestCommandValidator : AbstractValidator<PatchRequestCommand>
{
    public PatchRequestCommandValidator()
    {
        RuleFor(x => x.RequestId)
           .NotNull()
           .NotEmpty();
    }
}
