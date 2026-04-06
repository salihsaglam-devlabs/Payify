using FluentValidation;

namespace LinkPara.Approval.Application.Features.MakerCheckers.Queries.GetMakerChecker;
public class GetMakerCheckerQueryValidator : AbstractValidator<GetMakerCheckerQuery>
{
    public GetMakerCheckerQueryValidator()
    {
        RuleFor(s => s.Id)
            .NotNull()
            .NotEmpty();
    }
}