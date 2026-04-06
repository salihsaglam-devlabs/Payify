using FluentValidation;

namespace LinkPara.PF.Application.Features.Payments.Commands.GetThreeDSessionResult;

public class GetThreeDSessionResultCommandValidator : AbstractValidator <GetThreeDSessionResultCommand>
{
    public GetThreeDSessionResultCommandValidator()
    {
        RuleFor(s => s.ConversationId)
            .NotNull()
            .NotEmpty();

        RuleFor(s => s.ThreeDSessionId)
           .NotNull()
           .NotEmpty();

        RuleFor(s => s.MerchantId)
          .NotNull()
          .NotEmpty();
    }
}
