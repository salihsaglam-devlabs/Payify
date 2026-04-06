using FluentValidation;
using LinkPara.Emoney.Domain.Enums;
using Microsoft.Extensions.Localization;

namespace LinkPara.Emoney.Application.Features.Wallets.Commands.UpdateBalance;
public class UpdateBalanceCommandValidator : AbstractValidator<UpdateBalanceCommand>
{
    private readonly IStringLocalizer _localizer;
    public UpdateBalanceCommandValidator(IStringLocalizerFactory factory)
    {
        _localizer = factory.Create("Exceptions", "LinkPara.Emoney.API");

        RuleFor(x => x.CurrencyCode)
            .NotNull()
            .NotEmpty();

        RuleFor(x => x)
            .Must(x => { return x.Amount + x.CommissionAmount + x.FeeAmount > 0; })
            .WithMessage(_localizer.GetString("InvalidAmountException").Value);

        RuleFor(x => x.Amount)
            .Must((x, amount) =>
                x.TransactionType == TransactionType.Maintenance
                    ? amount >= 0
                    : amount > 0)
            .WithMessage(_localizer.GetString("InvalidAmountException").Value);

        RuleFor(x => x.Amount)
            .Must(v =>
            {
                var scale = (decimal.GetBits(v)[3] >> 16) & 0xFF;
                return scale <= 2;
            })
            .WithMessage(_localizer.GetString("AmountDigitException").Value);

        RuleFor(x => x.CommissionAmount)
            .GreaterThanOrEqualTo(0)
            .WithMessage(_localizer.GetString("InvalidAmountException").Value)
            .Must(v =>
            {
                var scale = (decimal.GetBits(v)[3] >> 16) & 0xFF;
                return scale <= 2;
            })
            .WithMessage(_localizer.GetString("AmountDigitException").Value);

        RuleFor(x => x.FeeAmount)
            .GreaterThanOrEqualTo(0)
            .WithMessage(_localizer.GetString("InvalidAmountException").Value)
            .Must(v =>
            {
                var scale = (decimal.GetBits(v)[3] >> 16) & 0xFF;
                return scale <= 2;
            })
            .WithMessage(_localizer.GetString("AmountDigitException").Value);

        RuleFor(x => x.Utid)
            .NotNull()
            .NotEmpty();

        RuleFor(x => x.Description).MaximumLength(300);

        RuleFor(x => x.WalletNumber)
            .NotNull()
            .NotEmpty()
            .MaximumLength(10);

        RuleFor(x => x.Channel).MaximumLength(300);

        RuleFor(x => x.TransactionDate)
            .Must(d => d != DateTime.MinValue && d != DateTime.MaxValue);
    }
}
