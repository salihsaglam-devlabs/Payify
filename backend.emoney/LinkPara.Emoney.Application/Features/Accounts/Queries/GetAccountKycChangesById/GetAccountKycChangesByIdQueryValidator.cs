using FluentValidation;

namespace LinkPara.Emoney.Application.Features.Accounts.Queries.GetAccountKycChangesById;

public class GetAccountKycChangesByIdQueryValidator : AbstractValidator<GetAccountKycChangesByIdQuery>
{
    public GetAccountKycChangesByIdQueryValidator()
    {
        RuleFor(s => s.Id)
            .NotNull()
            .NotEmpty();
    }
}