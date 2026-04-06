using FluentValidation;

namespace LinkPara.IKS.Application.Features.Terminal.Command.CreateTerminal;

public class CreateTerminalCommandValidator : AbstractValidator<CreateTerminalCommand>
{
    public CreateTerminalCommandValidator()
    {
        RuleFor(b => b.MerchantId).NotEmpty().NotNull();
        RuleFor(b => b.GlobalMerchantId).NotEmpty().NotNull().Length(8);
        RuleFor(b => b.PspMerchantId).NotEmpty().NotNull().MinimumLength(1).MaximumLength(15);
        RuleFor(b => b.OwnerPspNo).NotEmpty().NotNull();
        RuleFor(b => b.ServiceProviderPspNo).NotEmpty().NotNull();
        RuleFor(b => b.PfMainMerchantId).NotEmpty().NotNull();
        
        RuleFor(x => x.Type)
            .NotEmpty()
            .NotNull()
            .WithMessage("Type is required.");

        When(x => x.Type == "S", () =>
        {
            RuleFor(b => b.HostingTaxNo).NotEmpty().NotNull().MinimumLength(1).MaximumLength(11);
            RuleFor(b => b.HostingTradeName).NotEmpty().NotNull().MinimumLength(6).MaximumLength(150);
            RuleFor(b => b.HostingUrl).NotEmpty().NotNull().MinimumLength(1).MaximumLength(150);
            RuleFor(b => b.PaymentGwTaxNo).NotEmpty().NotNull().MinimumLength(1).MaximumLength(11);
            RuleFor(b => b.PaymentGwTradeName).NotEmpty().NotNull().MinimumLength(6).MaximumLength(150);
            RuleFor(b => b.PaymentGwUrl).NotEmpty().NotNull().MinimumLength(1).MaximumLength(150);
            RuleFor(b => b.VirtualPosUrl).NotEmpty().NotNull().MinimumLength(1).MaximumLength(150);
        });

        When(x => x.Type != "S", () =>
        {
            RuleFor(x => x.BrandCode).NotEmpty().NotNull();
            RuleFor(x => x.Model).NotEmpty().NotNull();
            RuleFor(x => x.SerialNo).NotEmpty().NotNull();
            RuleFor(x => x.Contactless).NotNull();
            RuleFor(x => x.PinPad).NotNull();
            RuleFor(x => x.ConnectionType).NotEmpty().NotNull();
            RuleFor(x => x.FiscalNo).NotEmpty().NotNull();
        });
            
    }
}
