using FluentValidation;

namespace LinkPara.PF.Application.Features.DeviceInventories.Command.DeleteDeviceInventory;

public class DeleteDeviceInventoryCommandValidator : AbstractValidator<DeleteDeviceInventoryCommand>
{
    public DeleteDeviceInventoryCommandValidator()
    {
        RuleFor(x => x.Id).NotNull().NotEmpty();
    }
}
