using FluentValidation;
using Microsoft.Extensions.Localization;
namespace LinkPara.Emoney.Application.Features.PayWithWallets.Commands.TransferForLoggedInUser;

public class TransferForLoggedInUserCommandValidator : AbstractValidator<TransferForLoggedInUserCommand>
{
    private readonly IStringLocalizer _localizer;
    public TransferForLoggedInUserCommandValidator(IStringLocalizerFactory factory)
    {
        _localizer = factory.Create("Exceptions", "LinkPara.Emoney.API");

        RuleFor(x => x.Amount)
            .NotNull()
            .WithMessage(_localizer.GetString("AmountCanNotBeEmptyException").Value);

        RuleFor(x => x.Amount)
          .GreaterThan(0)
          .WithMessage(_localizer.GetString("InvalidAmountException").Value);


        RuleFor(x => x.SenderPhoneNumber)
            .NotNull()
            .NotEmpty();


        RuleFor(x => x.PaymentReferenceId)
            .NotNull()
            .NotEmpty();


        RuleFor(x => x.PartnerNumber)
            .NotNull()
            .NotEmpty();

        RuleFor(x => x.Description)
           .MaximumLength(150);

    }
}