using FluentValidation;

namespace LinkPara.PF.Application.Features.SubMerchantUsers.Command.SaveSubMerchantUser;

public class SaveSubMerchantUserCommandValidator : AbstractValidator<SaveSubMerchantUserCommand>
{
    public SaveSubMerchantUserCommandValidator()
    {
        RuleFor(x => x.Name).MaximumLength(100)
            .WithMessage("Invalid User name!");

        RuleFor(x => x.Surname).MaximumLength(100)
            .WithMessage("Invalid User Surname!");

        RuleFor(x => x.Email).MaximumLength(256)
            .EmailAddress().WithMessage("Invalid User Email!");

        RuleFor(x => x.MobilePhoneNumber).NotNull().NotEmpty()
            .Must(x => x!.Trim().Length == 10)
            .WithMessage("Invalid User PhoneNumber format");

        RuleFor(x => x.RoleId).NotNull().NotEmpty();

        RuleFor(x => x.SubMerchantId).NotNull().NotEmpty();

        RuleFor(x => x.RoleName).NotNull().NotEmpty();
        RuleFor(x => x.IdentityNumber).NotNull().NotEmpty();
    }
}
