using FluentValidation;
using LinkPara.PF.Domain.Enums;
using System.Globalization;

namespace LinkPara.PF.Application.Features.Payments.Commands.Provision;

public class ProvisionCommandValidator : AbstractValidator<ProvisionCommand>
{
    public ProvisionCommandValidator()
    {
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

        RuleFor(s => s.CardToken)
            .NotNull()
            .NotEmpty()
            .When(s => s.PaymentType != VposPaymentType.PostAuth && s.IsOnUsPayment == false);

        RuleFor(s => s.MerchantId)
            .NotNull()
            .NotEmpty();

        RuleFor(s => s.Currency)
            .NotNull()
            .NotEmpty()
            .MaximumLength(3)
            .When(s => s.PaymentType != VposPaymentType.PostAuth);

        RuleFor(s => s.PaymentType)
            .IsInEnum();

        RuleFor(s => s.ConversationId)
            .NotNull()
            .NotEmpty();

        RuleFor(s => s.LanguageCode)
            .MaximumLength(2);

        RuleFor(s => s.InstallmentCount)
            .GreaterThanOrEqualTo(0)
            .LessThanOrEqualTo(36);

        RuleFor(s => s.OriginalOrderId)
            .NotNull()
            .NotEmpty()
            .When(s => s.PaymentType == VposPaymentType.PostAuth);

        RuleFor(s => s.CardHolderName)
            .NotNull()
            .NotEmpty()
            .MaximumLength(200)
            .When(s => s.PaymentType != VposPaymentType.PostAuth && s.IsOnUsPayment == false);
        
        RuleFor(s => s.CallbackUrl)
            .NotNull()
            .NotEmpty()
            .MaximumLength(250)
            .Must(BeAValidUrl)
            .When(s => s.IsOnUsPayment == true)
            .WithMessage("Valid CallbackUrl is required");
        
        RuleFor(s => s.MerchantCustomerPhoneCode)
            .NotNull()
            .NotEmpty()
            .When(s => s.IsOnUsPayment == true)
            .WithMessage("MerchantCustomerPhoneCode is required");
        
        RuleFor(s => s.MerchantCustomerPhoneNumber)
            .NotNull()
            .NotEmpty()
            .When(s => s.IsOnUsPayment == true)
            .WithMessage("MerchantCustomerPhoneNumber is required");
    }

    private bool BeAValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult) && 
               (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
}
