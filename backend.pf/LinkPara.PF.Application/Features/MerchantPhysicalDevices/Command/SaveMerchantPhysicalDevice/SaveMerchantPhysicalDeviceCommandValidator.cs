using FluentValidation;
using LinkPara.PF.Application.Commons.Models.MerchantPhysicalDevices;

namespace LinkPara.PF.Application.Features.MerchantPhysicalDevices.Command.SaveMerchantPhysicalDevice;

public class SaveMerchantPhysicalDeviceCommandValidator : AbstractValidator<SaveMerchantPhysicalDeviceCommand>
{
    public SaveMerchantPhysicalDeviceCommandValidator()
    {
        RuleFor(x => x.MerchantId).NotNull().NotEmpty();

        RuleForEach(x => x.SaveMerchantPhysicalDeviceList)
             .SetValidator(new SaveMerchantPhysicalDeviceValidator());

        RuleFor(x => x.PhysicalPosIdList)
             .NotEmpty().NotEmpty()
             .WithMessage("Physical Pos cannot be empty");
    }

    public class SaveMerchantPhysicalDeviceValidator : AbstractValidator<SaveMerchantPhysicalDeviceRequest>
    {
        public SaveMerchantPhysicalDeviceValidator()
        {
            RuleFor(x => x.DeviceInventoryId).NotNull().NotEmpty();

            RuleFor(x => x.AssignmentType).IsInEnum();

            RuleFor(x => x.ConnectionType).IsInEnum();

            RuleFor(x => x.FiscalNo).NotNull().NotEmpty();
        }
    }
}
