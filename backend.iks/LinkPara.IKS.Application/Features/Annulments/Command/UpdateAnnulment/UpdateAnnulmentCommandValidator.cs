using FluentValidation;


namespace LinkPara.IKS.Application.Features.Annulments.Command.UpdateAnnulment
{
    public class UpdateAnnulmentCommandValidator :  AbstractValidator<UpdateAnnulmentCommand>
    {
        public UpdateAnnulmentCommandValidator()
        {
            RuleFor(b => b.GlobalMerchantId).NotEmpty().NotNull().Length(8);
            RuleFor(b => b.Code).NotEmpty().NotNull();
            RuleFor(b => b.MerchantId).NotEmpty().NotNull();
            RuleFor(b => b.OwnerIdentityNo).NotEmpty().NotNull().MinimumLength(10).MaximumLength(11);
        }
    }
}
