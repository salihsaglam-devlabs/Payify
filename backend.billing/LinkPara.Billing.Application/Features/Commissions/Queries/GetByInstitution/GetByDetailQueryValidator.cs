using FluentValidation;

namespace LinkPara.Billing.Application.Features.Commissions.Queries.GetByDetail;

public class GetByDetailQueryValidator : AbstractValidator<GetByDetailQuery>
{
    public GetByDetailQueryValidator()
    {
        RuleFor(p => p.InstitutionId).NotEmpty().NotNull();
        RuleFor(p => p.PaymentSource).NotEmpty().NotNull();
        RuleFor(p => p.Amount).NotEmpty().NotNull();
    }
}