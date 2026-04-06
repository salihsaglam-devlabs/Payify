using FluentValidation;

namespace LinkPara.Approval.Application.Features.Cases.Queries.GetById;

public class GetByIdQueryValidator : AbstractValidator<GetByIdQuery>
{
    public GetByIdQueryValidator()
    {
        RuleFor(s => s.Id)
            .NotNull()
            .NotEmpty();
    }
}
