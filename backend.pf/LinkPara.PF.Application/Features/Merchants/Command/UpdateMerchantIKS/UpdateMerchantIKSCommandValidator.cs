using FluentValidation;


namespace LinkPara.PF.Application.Features.Merchants.Command.UpdateMerchantIKS
{
    public class UpdateMerchantIKSCommandValidator : AbstractValidator<UpdateMerchantIKSCommand>
    {
        public UpdateMerchantIKSCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty().NotNull();
            RuleFor(x => x.MerchantStatus).IsInEnum();
            RuleFor(x => x.GlobalMerchantId).NotNull().NotEmpty().Length(8);
        }
    }
}
