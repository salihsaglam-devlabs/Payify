using FluentValidation;


namespace LinkPara.PF.Application.Features.MerchantLimits.Command.SaveMerchantLimit
{
    public class SaveMerchantLimitCommandValidator : AbstractValidator<SaveMerchantLimitCommand>
    {
        public SaveMerchantLimitCommandValidator()
        {
            RuleFor(x => x.MerchantId).NotNull().NotEmpty();
            RuleFor(x => x.Currency).MaximumLength(50);
        }
    }
}
