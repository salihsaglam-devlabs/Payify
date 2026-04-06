using FluentValidation;
using System.Globalization;

namespace LinkPara.PF.Application.Features.Payments.Commands.Return;

public class ReturnCommandValidator : AbstractValidator<ReturnCommand>
{
    public ReturnCommandValidator()
    {
        RuleFor(s => s.MerchantId)
            .NotNull()
            .NotEmpty();

        RuleFor(s => s.ConversationId)
            .NotNull()
            .NotEmpty();

        RuleFor(s => s.OrderId)
            .NotNull()
            .NotEmpty();

        RuleFor(s => s.Amount)
            .NotEmpty()
            .NotNull()
            .GreaterThan(0.99m)
            .WithMessage("Amount must be greater and equal than 1.00.")
            .Must(a =>
            {
                decimal fractionalPart = a - Math.Floor(a);
                string[] parts = fractionalPart.ToString(CultureInfo.InvariantCulture).Split('.');
                return fractionalPart == 0 || parts[1].Length <= 2;
            })
            .WithMessage("Amount must have a maximum of 2 decimal places.");

        RuleFor(s => s.LanguageCode)
            .MaximumLength(2);
    }
}