using FluentValidation;

namespace LinkPara.Identity.Application.Features.DeviceInfos.Commands.DeleteUserDeviceCommand;

public class DeleteUserDeviceCommandValidator : AbstractValidator<DeleteUserDeviceCommand>
{
    public DeleteUserDeviceCommandValidator()
    {
        RuleFor(u => u.DeviceId)
            .NotNull().NotEmpty();
    }
}