using FluentValidation;
using Microsoft.Extensions.Localization;

namespace LinkPara.Emoney.Application.Features.TransferOrders.Commands.CreateTransferOrder;

public class CreateTransferOrderCommandValidator : AbstractValidator<CreateTransferOrderCommand>
{
    private readonly IStringLocalizer _localizer;
    public CreateTransferOrderCommandValidator(IStringLocalizerFactory factory)
    {
        _localizer = factory.Create("Exceptions", "LinkPara.Emoney.API");

        RuleFor(x => x.SenderWalletNumber)
            .MaximumLength(10)
            .NotEmpty()
            .NotNull();

        RuleFor(x => x.ReceiverAccountType)
            .IsInEnum();

        RuleFor(x => x.ReceiverAccountValue)
            .MaximumLength(50)
            .NotEmpty()
            .NotNull();

        RuleFor(x => x.Amount)
            .NotNull()
            .WithMessage(_localizer.GetString("AmountCanNotBeEmptyException").Value);

        RuleFor(x => x.TransferDate)
            .NotEmpty()
            .NotNull()
            .Must(IsValidDate);

        When(x => x.ReceiverAccountType == Domain.Enums.ReceiverAccountType.Iban, () =>
        {
            RuleFor(q => q.ReceiverNameSurname).MaximumLength(50).NotEmpty().NotNull();
            RuleFor(q => q.ReceiverAccountValue).Length(26);
        });

        When(x => x.ReceiverAccountType == Domain.Enums.ReceiverAccountType.PhoneNumber, () =>
        {
            RuleFor(q => q.ReceiverPhoneCode).MaximumLength(10).NotEmpty().NotNull();
            RuleFor(q => q.ReceiverAccountValue).Length(10);
        });
    }

    private static bool IsValidDate(DateTime date)
    {
        return date.Date >= DateTime.Now.AddDays(1).Date &&
                date.Date <= DateTime.Now.AddDays(32).Date;
    }

}
