using FluentValidation;

namespace LinkPara.Identity.Application.Features.DeviceInfos.Commands.CreateDeviceInfo;

public class CreateDeviceInfoCommandValidator : AbstractValidator<CreateDeviceInfoCommand>
{
    public CreateDeviceInfoCommandValidator()
    {
        RuleFor(u => u.RegistrationToken)
            .NotNull().NotEmpty();
    }
}