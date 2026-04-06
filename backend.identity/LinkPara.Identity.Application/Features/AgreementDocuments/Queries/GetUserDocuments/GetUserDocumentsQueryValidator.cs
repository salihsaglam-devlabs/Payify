using FluentValidation;

namespace LinkPara.Identity.Application.Features.AgreementDocuments.Queries.GetUserDocuments;

public class GetUserDocumentsQueryValidator : AbstractValidator<GetUserDocumentsQuery>
{
    public GetUserDocumentsQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotNull()
            .NotEmpty();
    }
}