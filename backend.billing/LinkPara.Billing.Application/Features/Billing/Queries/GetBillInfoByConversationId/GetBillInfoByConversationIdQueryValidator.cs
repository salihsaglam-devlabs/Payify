using FluentValidation;

namespace LinkPara.Billing.Application.Features.Billing.Queries.GetBillInfoByConversationId;

public class GetBillInfoByConversationIdQueryValidator : AbstractValidator<GetBillInfoByConversationIdQuery>
{
    public GetBillInfoByConversationIdQueryValidator()
    {
        RuleFor(s => s.ConversationId).NotEmpty().NotNull();
    }
}