using FluentValidation;


namespace LinkPara.PF.Application.Features.MerchantReturnPools.Command.ActionMerchantReturnPool
{
    public class ActionMerchantReturnPoolCommandValidator : AbstractValidator<ActionMerchantReturnPoolCommand>
    {
        public ActionMerchantReturnPoolCommandValidator()
        {
            RuleFor(b => b.MerchantReturnPoolId)
                .NotEmpty()
                .NotNull();
            RuleFor(b => b.ReturnStatus).IsInEnum();
        }
    }
}
