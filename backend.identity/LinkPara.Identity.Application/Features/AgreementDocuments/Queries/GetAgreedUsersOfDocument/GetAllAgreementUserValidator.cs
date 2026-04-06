using FluentValidation;

namespace LinkPara.Identity.Application.Features.AgreementDocuments.Queries.GetAgreedUsersOfDocument
{
    public class GetAgreedUsersOfDocumentQueryValidator : AbstractValidator<GetAgreedUsersOfDocumentQuery>
    {
        public GetAgreedUsersOfDocumentQueryValidator()
        {
            RuleFor(s => s.Id).NotNull().NotEmpty();
        }
    }
}