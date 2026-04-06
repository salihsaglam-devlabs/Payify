using FluentValidation;

namespace LinkPara.Accounting.Application.Features.Payments.Queries.GetPaymentById;

public class GetPaymentByIdQueryValidator : AbstractValidator<GetPaymentByIdQuery>
{
    public GetPaymentByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .NotNull();
    }
}
