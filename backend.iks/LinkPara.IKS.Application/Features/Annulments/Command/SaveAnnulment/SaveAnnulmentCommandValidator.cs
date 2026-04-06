using FluentValidation;
using LinkPara.IKS.Application.Features.Annuments.Command.SaveAnnulment;

namespace LinkPara.IKS.Application.Features.Annulments.Command.SaveAnnulment
{
    public class SaveAnnulmentCommandValidator : AbstractValidator<SaveAnnulmentCommand>
    {
        public SaveAnnulmentCommandValidator()
        {
            RuleFor(b => b.GlobalMerchantId).NotEmpty().NotNull().Length(8);
            RuleFor(b => b.Code).NotEmpty().NotNull();
            RuleFor(b => b.MerchantId).NotEmpty().NotNull();
            RuleFor(b => b.OwnerIdentityNo).NotEmpty().NotNull().MinimumLength(10).MaximumLength(11);
        }
    }
}