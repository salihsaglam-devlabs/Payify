using FluentValidation;

namespace LinkPara.PF.Application.Features.Banks.Queries.GetBankApiKey;

public class GetBankApiKeyQueryValidator : AbstractValidator<GetBankApiKeyQuery>
{
    public GetBankApiKeyQueryValidator()
    {
        RuleFor(s => s.AcquireBankId).NotNull().NotEmpty();
    }
}
