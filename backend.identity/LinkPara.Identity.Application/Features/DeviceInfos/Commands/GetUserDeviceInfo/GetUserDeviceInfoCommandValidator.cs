using FluentValidation;

namespace LinkPara.Identity.Application.Features.DeviceInfos.Commands.GetUserDeviceInfoCommand;

public class GetUserDeviceInfoCommandValidator : AbstractValidator<GetUserDeviceInfoCommand>
{
    public GetUserDeviceInfoCommandValidator()
    {
        RuleFor(u => u.UserIdList)
            .NotNull().NotEmpty();
    }
}