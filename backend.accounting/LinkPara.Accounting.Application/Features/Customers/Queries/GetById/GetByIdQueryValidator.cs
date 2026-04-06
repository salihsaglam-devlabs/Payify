using FluentValidation;

namespace LinkPara.Accounting.Application.Features.Customers.Queries.GetById;

public class GetByIdQueryValidator : AbstractValidator<GetByIdQuery>
{
    public GetByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .NotNull();
    }
}
