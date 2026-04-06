using FluentValidation;

namespace LinkPara.Emoney.Application.Features.CompanyPools.Commands.ApproveCompanyPool;

public class ApproveCompanyPoolCommandValidator : AbstractValidator<ApproveCompanyPoolCommand>
{
    public ApproveCompanyPoolCommandValidator()
    {
        RuleFor(x=>x.CompanyPoolId)
            .NotNull()
            .NotEmpty();
        RuleFor(x => x.UserId)
            .NotNull()
            .NotEmpty();
    }
}
