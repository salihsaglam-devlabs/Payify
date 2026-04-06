using FluentValidation;

namespace LinkPara.Billing.Application.Features.Commissions.Queries.GetById;

public class GetCommissionByIdValidator : AbstractValidator<GetCommissionByIdQuery>
{
    public GetCommissionByIdValidator()
    {
        RuleFor(p => p.CommissionId)
            .NotEmpty().WithMessage("{PropertyName} is required.")
            .NotNull();
    }
}