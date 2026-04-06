using FluentValidation;

namespace LinkPara.PF.Application.Features.MerchantBusinessPartners.Command.UpdateMerchantBusinessPartner
{
    public class UpdateMerchantBusinessPartnerCommandValidator : AbstractValidator<UpdateMerchantBusinessPartnerCommand>
    {
        public UpdateMerchantBusinessPartnerCommandValidator()
        {
            RuleFor(b => b.Id).NotNull().NotEmpty();

            RuleFor(x => x.FirstName).NotEmpty().NotNull()
           .MaximumLength(100);

            RuleFor(x => x.LastName).NotEmpty().NotNull()
            .MaximumLength(100);

            RuleFor(x => x.Email).NotEmpty().NotNull()
            .MaximumLength(256);

            RuleFor(x => x.PhoneNumber).NotNull().NotEmpty()
               .Must(x => x!.Trim().Length == 10);

            RuleFor(x => x.MerchantId).NotNull().NotEmpty();
        }
    }
}
