using FluentValidation;

namespace LinkPara.PF.Application.Features.MerchantPhysicalDevices.Command.UpdateMerchantPhysicalDevice;

public class UpdateMerchantPhysicalDeviceCommandValidator : AbstractValidator<UpdateMerchantPhysicalDeviceCommand>
{
    public UpdateMerchantPhysicalDeviceCommandValidator()
    {
        RuleFor(x => x.Id).NotNull().NotEmpty();

        RuleFor(x => x.DeviceStatus).IsInEnum();
    }
}
