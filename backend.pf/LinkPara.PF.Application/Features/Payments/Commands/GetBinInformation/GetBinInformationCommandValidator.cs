using FluentValidation;

namespace LinkPara.PF.Application.Features.Payments.Commands.GetBinInformation;

public class GetBinInformationCommandValidator : AbstractValidator<GetBinInformationCommand>
{
    public GetBinInformationCommandValidator()
    {
        RuleFor(s => s.CardToken)
            .NotNull()
            .NotEmpty()
            .When(s => string.IsNullOrEmpty(s.BinNumber));

        RuleFor(s => s.BinNumber)
            .NotNull()
            .NotEmpty()
            .Matches(@"^\d{6}$")
            .When(s => string.IsNullOrEmpty(s.CardToken));

        RuleFor(s => s.LanguageCode)
            .MaximumLength(2);

        RuleFor(s => s.MerchantId)
            .NotNull()
            .NotEmpty();

        RuleFor(s => s.ConversationId)
            .NotNull()
            .NotEmpty();

        RuleFor(s => s)
            .Custom((req, context) =>
            {
                if (!string.IsNullOrEmpty(req.CardToken) && !string.IsNullOrEmpty(req.BinNumber))
                {
                    context.AddFailure("Only 'CardToken' or 'BinNumber' parameter must be entered");
                }
            });
    }
}