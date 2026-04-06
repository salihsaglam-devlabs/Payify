using FluentValidation;

namespace LinkPara.Identity.Application.Features.AgreementDocuments.Queries.GetDocuments;

public class GetAllDocumentsQueryValidator : AbstractValidator<GetDocumentsQuery>
{
    public GetAllDocumentsQueryValidator()
    {
        RuleFor(x => x.AgreementDocumentId)
            .NotNull()
            .NotEmpty();
    }
}