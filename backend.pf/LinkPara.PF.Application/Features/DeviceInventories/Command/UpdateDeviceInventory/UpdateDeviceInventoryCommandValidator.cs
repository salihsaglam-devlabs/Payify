using FluentValidation;

namespace LinkPara.PF.Application.Features.DeviceInventories.Command.UpdateDeviceInventory;

public class UpdateDeviceInventoryCommandValidator : AbstractValidator<UpdateDeviceInventoryCommand>
{
    public UpdateDeviceInventoryCommandValidator()
    {
        RuleFor(x => x.SerialNo).NotNull().NotEmpty()
      .WithMessage("Invalid SerialNo!");

        RuleFor(x => x.ContactlessSeparator).IsInEnum();
        RuleFor(x => x.DeviceModel).IsInEnum();
        RuleFor(x => x.PhysicalPosVendor).IsInEnum();
        RuleFor(x => x.DeviceStatus).IsInEnum();
        RuleFor(x => x.InventoryType).IsInEnum();
        RuleFor(x => x.DeviceType).IsInEnum();
    }
}
