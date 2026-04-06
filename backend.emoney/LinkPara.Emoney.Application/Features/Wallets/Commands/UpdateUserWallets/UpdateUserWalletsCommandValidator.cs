using FluentValidation;

namespace LinkPara.Emoney.Application.Features.Wallets.Commands.UpdateUserWallets
{
    public class UpdateUserWalletsCommandValidator : AbstractValidator<UpdateUserWalletsCommand>
    {
        public UpdateUserWalletsCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotNull()
                .NotEmpty();

            RuleFor(x => x.IsBlockage)
               .NotNull();


            RuleFor(x => x.RecordStatus)
               .IsInEnum();
        }
    }
}