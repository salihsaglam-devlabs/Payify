
using FluentValidation;

namespace LinkPara.Emoney.Application.Features.Provisions.Queries.InquireProvision;

public class InquireProvisionQueryValidator : AbstractValidator<InquireProvisionQuery>
{
    public InquireProvisionQueryValidator()
    {
        RuleFor(x => x.ConversationId)
            .NotNull().When(x => string.IsNullOrEmpty(x.ProvisionReference))
            .NotEmpty().When(x => string.IsNullOrEmpty(x.ProvisionReference));
        RuleFor(x => x.ProvisionReference)
            .NotNull().When(x => string.IsNullOrEmpty(x.ConversationId))
            .NotEmpty().When(x => string.IsNullOrEmpty(x.ConversationId));
    }
}
