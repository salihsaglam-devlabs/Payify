using FluentValidation;

namespace LinkPara.PF.Application.Features.AcquireBanks.Queries.GetAcquireBankById;

public class GetAcquireBankByIdQueryValidator : AbstractValidator<GetAcquireBankByIdQuery>
{
    public GetAcquireBankByIdQueryValidator()
    {
        RuleFor(x => x.Id)
      .NotNull().NotEmpty();
    }
}
