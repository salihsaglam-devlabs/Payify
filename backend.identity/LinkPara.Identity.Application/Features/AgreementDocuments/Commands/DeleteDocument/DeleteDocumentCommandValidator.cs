using FluentValidation;

namespace LinkPara.Identity.Application.Features.AgreementDocuments.Commands.DeleteDocument;
    public class DeleteDocumentCommandValidator : AbstractValidator<DeleteDocumentCommand>
    {
        public DeleteDocumentCommandValidator()
        {
            RuleFor(u => u.Id)
                .NotNull().NotEmpty();
        }
    }
