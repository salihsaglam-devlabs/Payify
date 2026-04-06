using FluentValidation;

namespace LinkPara.PF.Application.Features.Tokens.Commands.GenerateCardToken;

public class GenerateCardTokenCommandValidator : AbstractValidator<GenerateCardTokenCommand>
{
    public GenerateCardTokenCommandValidator()
    {
        RuleFor(s => s.ExpireMonth).Length(2);
        RuleFor(s => s.ExpireYear).Length(2);
        RuleFor(s => s.Cvv).MaximumLength(3);
        RuleFor(s => s.CardNumber).MaximumLength(19);
        RuleFor(s => s.CardNumber).MinimumLength(10);
    }
}