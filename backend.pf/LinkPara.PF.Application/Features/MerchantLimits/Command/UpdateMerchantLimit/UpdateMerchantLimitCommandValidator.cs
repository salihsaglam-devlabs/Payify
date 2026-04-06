using FluentValidation;


namespace LinkPara.PF.Application.Features.MerchantLimits.Command.UpdateMerchantLimit
{
    public class UpdateMerchantLimitCommandValidator : AbstractValidator<UpdateMerchantLimitCommand>
    {
        public UpdateMerchantLimitCommandValidator()
        {
            RuleFor(x => x.Id).NotNull().NotEmpty();
            RuleFor(x => x.MerchantId).NotNull().NotEmpty();
            RuleFor(x => x.Currency).MaximumLength(50);
        }
    }
}
