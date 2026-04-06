using FluentValidation;

namespace LinkPara.Emoney.Application.Features.ConsentOperations.Queries.GetConsentDetail;

public class GetConsentDetailQueryValidator : AbstractValidator<GetConsentDetailQuery>
{
    public GetConsentDetailQueryValidator()
    {
        RuleFor(x => x.ConsentId).NotEmpty().NotNull();
    }
}
