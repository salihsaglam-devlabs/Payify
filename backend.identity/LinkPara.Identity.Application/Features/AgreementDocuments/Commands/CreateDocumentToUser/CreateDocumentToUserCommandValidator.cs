using FluentValidation;

namespace LinkPara.Identity.Application.Features.AgreementDocuments.Commands.CreateDocumentToUser;

public class CreateDocumentToUserCommandValidator : AbstractValidator<CreateDocumentToUserCommand>
{
    public CreateDocumentToUserCommandValidator()
    {
        RuleFor(x => x.AgreementDocumentId)
            .NotNull()
            .NotEmpty();
        RuleFor(x => x.UserId)
            .NotNull()
            .NotEmpty();
    }
}