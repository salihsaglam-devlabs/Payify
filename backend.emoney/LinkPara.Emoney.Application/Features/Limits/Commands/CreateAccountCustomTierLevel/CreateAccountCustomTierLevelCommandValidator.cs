using FluentValidation;

namespace LinkPara.Emoney.Application.Features.Limits.Commands.CreateAccountCustomTierLevel
{
    public class CreateAccountCustomTierLevelCommandValidator : AbstractValidator<CreateAccountCustomTierLevelCommand>
    {
        public CreateAccountCustomTierLevelCommandValidator()
        {
            RuleFor(u => u.AccountCustomTierDto.AccountId)
                .NotNull()
                .NotEmpty();

            RuleFor(u => u.AccountCustomTierDto.TierLevelId)
                .NotNull()
                .NotEmpty();

            RuleFor(u => u.AccountCustomTierDto.AccountName)
                .MaximumLength(200)
                .NotNull()
                .NotEmpty();

            RuleFor(u => u.AccountCustomTierDto.PhoneCode)
                .MaximumLength(10)
                .NotNull()
                .NotEmpty();

            RuleFor(u => u.AccountCustomTierDto.PhoneNumber)
                .MaximumLength(50)
                .NotNull()
                .NotEmpty();

            RuleFor(u => u.AccountCustomTierDto.Email)
                .MaximumLength(250)
                .NotNull()
                .NotEmpty();
        }
    }
}