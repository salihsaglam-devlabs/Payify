using FluentValidation;

namespace LinkPara.Approval.Application.Features.MakerCheckers.Queries.GetMakerCheckersByCaseId;

public class GetMakerCheckersByCaseIdQueryValidator : AbstractValidator<GetMakerCheckersByCaseIdQuery>
{
    public GetMakerCheckersByCaseIdQueryValidator()
    {
        RuleFor(s => s.CaseId)
            .NotNull()
            .NotEmpty();
    }
}
