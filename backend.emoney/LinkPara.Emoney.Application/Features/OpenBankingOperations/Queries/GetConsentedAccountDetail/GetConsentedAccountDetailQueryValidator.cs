using FluentValidation;

namespace LinkPara.Emoney.Application.Features.OpenBankingOperations.Queries.GetConsentedAccountDetail;

public class GetConsentedAccountDetailQueryValidator : AbstractValidator<GetConsentedAccountDetailQuery>
{
    public GetConsentedAccountDetailQueryValidator()
    {
    //    RuleFor(x => x.AccountReference).NotEmpty().NotNull();
    //    RuleFor(x => x.HhsCode).NotEmpty().NotNull();
    //    RuleFor(x => x.AppUserId).NotEmpty().NotNull();
    }
}
