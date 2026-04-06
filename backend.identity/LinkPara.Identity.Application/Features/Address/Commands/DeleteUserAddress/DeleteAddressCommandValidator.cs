using FluentValidation;

namespace LinkPara.Identity.Application.Features.Address.Commands.DeleteUserAddress;

public class DeleteAddressCommandValidator : AbstractValidator<DeleteAddressCommand>
{
    public DeleteAddressCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotNull().NotEmpty();
    }
}