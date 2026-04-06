using FluentValidation;

namespace LinkPara.PF.Application.Features.DeviceInventories.Queries.GetDeviceInventoryById;

public class GetDeviceInventoryByIdQueryValidator : AbstractValidator<GetDeviceInventoryByIdQuery>
{
    public GetDeviceInventoryByIdQueryValidator()
    {
        RuleFor(x => x.Id).NotNull().NotEmpty();
    }
}
