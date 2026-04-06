using FluentValidation;

namespace LinkPara.Emoney.Application.Features.ConsentOperations.Queries.GetWaitingApprovalConsents;

public class GetWaitingApprovalConsentQueryValidator : AbstractValidator<GetWaitingApprovalConsentQuery>
{
    public GetWaitingApprovalConsentQueryValidator()
    {
        RuleFor(x => x.AccountId).NotEmpty().NotNull();

    }
}
