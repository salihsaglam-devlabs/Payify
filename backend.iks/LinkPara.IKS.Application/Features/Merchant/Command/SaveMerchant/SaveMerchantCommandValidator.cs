using FluentValidation;

namespace LinkPara.IKS.Application.Features.Merchant.Command.SaveMerchant
{
    public class SaveMerchantCommandValidator : AbstractValidator<SaveMerchantCommand>
    {
        public SaveMerchantCommandValidator()
        {
            RuleFor(b => b.MerchantId).NotEmpty().NotNull();
            RuleFor(b => b.PspMerchantId).NotEmpty().NotNull().MinimumLength(1).MaximumLength(15);
            RuleFor(b => b.TaxNo).NotEmpty().NotNull().MinimumLength(10).MaximumLength(11);
            RuleFor(b => b.TradeName).NotEmpty().NotNull();
            RuleFor(b => b.MerchantName).NotEmpty().NotNull();
            RuleFor(b => b.Address).NotEmpty().NotNull();
            RuleFor(b => b.LicenseTag).NotEmpty().NotNull();
            RuleFor(b => b.Mcc).NotEmpty().NotNull();
            RuleFor(b => b.CommercialType).NotEmpty().NotNull().Length(1);
            RuleFor(b => b.TaxOfficeName).MinimumLength(1).MaximumLength(140);
            RuleFor(b => b.ZipCode).MinimumLength(3).MaximumLength(10);
            RuleFor(b => b.ManagerName).NotEmpty().NotNull()
                .MinimumLength(6).MaximumLength(140);
        }
    }
}
