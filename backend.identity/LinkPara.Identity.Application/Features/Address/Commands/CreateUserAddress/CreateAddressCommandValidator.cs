using FluentValidation;

namespace LinkPara.Identity.Application.Features.Address.Commands.CreateUserAddress;

public class CreateAddressCommandValidator : AbstractValidator<CreateAddressCommand>
{
    public CreateAddressCommandValidator()
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