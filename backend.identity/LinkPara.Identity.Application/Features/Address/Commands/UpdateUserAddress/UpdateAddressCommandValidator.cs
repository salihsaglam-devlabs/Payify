using FluentValidation;

namespace LinkPara.Identity.Application.Features.Address.Commands.UpdateUserAddress;

public class UpdateAddressCommandValidator : AbstractValidator<UpdateAddressCommand>
{
    public UpdateAddressCommandValidator()
    {
        RuleFor(x => x.CountryId)
            .NotNull().NotEmpty();

        RuleFor(x => x.CityId)
            .NotNull().NotEmpty();

        RuleFor(x => x.DistrictId)
            .NotNull().NotEmpty();

        RuleFor(x => x.Neighbourhood)
            .NotNull().NotEmpty().MaximumLength(450);

        RuleFor(x => x.Street)
            .NotNull().NotEmpty().MaximumLength(450);

        RuleFor(x => x.FullAddress)
            .NotNull().NotEmpty().MaximumLength(600);

    }
}