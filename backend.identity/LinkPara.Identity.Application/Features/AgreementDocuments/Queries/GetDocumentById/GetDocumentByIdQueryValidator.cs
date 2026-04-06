using FluentValidation;

namespace LinkPara.Identity.Application.Features.AgreementDocuments.Queries.GetDocumentById;
public class GetDocumentByIdQueryValidator : AbstractValidator<GetDocumentByIdQuery>
{
    public GetDocumentByIdQueryValidator()
    {
        RuleFor(s => s.AgreementDocumentId).NotNull().NotEmpty();
    }
}
