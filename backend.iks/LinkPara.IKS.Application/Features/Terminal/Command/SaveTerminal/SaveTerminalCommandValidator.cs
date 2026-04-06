using FluentValidation;


namespace LinkPara.IKS.Application.Features.Terminal.Command.SaveTerminal
{
    public class SaveTerminalCommandValidator : AbstractValidator<SaveTerminalCommand>
    {
        public SaveTerminalCommandValidator()
        {
            RuleFor(b => b.MerchantId).NotEmpty().NotNull();
            RuleFor(b => b.GlobalMerchantId).NotEmpty().NotNull().Length(8);
            RuleFor(b => b.PspMerchantId).NotEmpty().NotNull().MinimumLength(1).MaximumLength(15);
            RuleFor(b => b.TerminalId).NotEmpty().NotNull()
                .MinimumLength(1).MaximumLength(8);

            RuleFor(b => b.BrandSharing).NotEmpty().NotNull().Length(1);
            RuleFor(b => b.OwnerPspNo).NotEmpty().NotNull();
            RuleFor(b => b.VirtualPosUrl).NotEmpty().NotNull()
                .MinimumLength(1).MaximumLength(150);
            RuleFor(b => b.HostingTaxNo).NotEmpty().NotNull()
               .MinimumLength(1).MaximumLength(11);
            RuleFor(b => b.PaymentGwTaxNo).NotEmpty().NotNull()
              .MinimumLength(1).MaximumLength(11);

            RuleFor(b => b.ServiceProviderPspNo).NotEmpty().NotNull();
        }
    }
}
