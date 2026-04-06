using FluentValidation;

namespace LinkPara.Identity.Application.Features.AgreementDocuments.Commands.CreateDocument;

public class CreateDocumentCommandValidator : AbstractValidator<CreateDocumentCommand>
{
    public CreateDocumentCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotNull()
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.Agreements)
            .NotNull()
            .NotEmpty();

        RuleForEach(x => x.Agreements)
         .SetValidator(new CreateDocumentVersionValidator());
    }

    public class CreateDocumentVersionValidator : AbstractValidator<DocumentVersionDto>
    {
        public CreateDocumentVersionValidator()
        {

            RuleFor(x => x.Title)
               .NotNull().NotEmpty()
               .MaximumLength(150);
        }
    }
}