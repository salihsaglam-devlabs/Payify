using FluentValidation;
using LinkPara.PF.Domain.Enums;

namespace LinkPara.PF.Application.Features.HostedPayments.Command.InitHostedPayment;

public class InitHostedPaymentCommandValidator : AbstractValidator<InitHostedPaymentCommand>
{
    public InitHostedPaymentCommandValidator()
    {
        RuleFor(b => b.MerchantId)
            .NotNull()
            .NotEmpty();
        
        RuleFor(b => b.OrderId)
            .NotNull()
            .NotEmpty()
            .MaximumLength(24);
        
        RuleFor(x => x.Amount)
            .NotNull()
            .NotEmpty();

        RuleFor(b => b.Currency)
            .NotNull()
            .NotEmpty();
        
        RuleFor(x => x.PageViewType)
            .IsInEnum();
        
        RuleFor(b => b.CallbackUrl)
            .NotNull()
            .NotEmpty()
            .MaximumLength(250);
        
        When(x => x.PageViewType == HppPageViewType.Redirect, () =>
        {
            RuleFor(b => b.ReturnUrl)
                .NotNull()
                .NotEmpty()
                .MaximumLength(250)
                .WithMessage("ReturnUrl required when PageView is Redirect");
        });
        
        RuleFor(x => x.Is3dRequired)
            .NotNull();
        
        RuleFor(b => b.Name)
            .MaximumLength(150);
        
        RuleFor(b => b.Surname)
            .MaximumLength(150);
        
        RuleFor(x => x.Email)
            .MaximumLength(256)
            .EmailAddress()
            .WithMessage("Invalid Email!");
        
        RuleFor(x => x.PhoneNumber)
            .NotNull()
            .NotEmpty()
            .Must(x => x!.Trim().Length == 10)
            .WithMessage("Invalid PhoneNumber!");
        
        When(x => !string.IsNullOrEmpty(x.LanguageCode), () =>
        {
            RuleFor(x => x.LanguageCode)
                .Must(x => x is "TR" or "EN")
                .WithMessage("LanguageCode must be either 'EN' or 'TR'");
        });
        
        RuleFor(b => b.ClientIpAddress)
            .NotNull()
            .NotEmpty()
            .MaximumLength(50);
    }
}