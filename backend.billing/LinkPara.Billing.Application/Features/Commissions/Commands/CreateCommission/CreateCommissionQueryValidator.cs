using FluentValidation;

namespace LinkPara.Billing.Application.Features.Commissions.Commands.CreateCommission;

public class CreateCommissionQueryValidator : AbstractValidator<CreateCommissionQuery>
{
    public CreateCommissionQueryValidator()
    {
        RuleFor(c => c.Fee).NotNull();
        RuleFor(c => c.Rate).NotNull();
        RuleFor(c => c.VendorId).NotEmpty().NotNull();
        RuleFor(c => c.MinValue).NotNull();
        RuleFor(c => c.MaxValue).NotEmpty().NotNull();
    }
}