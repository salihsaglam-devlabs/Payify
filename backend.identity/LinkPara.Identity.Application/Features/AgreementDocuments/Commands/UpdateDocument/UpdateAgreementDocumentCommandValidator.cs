using FluentValidation;

namespace LinkPara.Identity.Application.Features.AgreementDocuments.Commands.UpdateDocument;

public class UpdateAgreementDocumentCommandValidator : AbstractValidator<UpdateAgreementDocumentCommand>
{
    public UpdateAgreementDocumentCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotNull()
            .NotEmpty();

        RuleFor(x => x.Agreements)
            .NotNull()
            .NotEmpty();

    }
}