using FluentValidation;

namespace LinkPara.Emoney.Application.Features.CorporateWallets.Commands.UpdateCorporateAccount;

public class UpdateCorporateAccountCommandValidator : AbstractValidator<UpdateCorporateAccountCommand>
{
    public UpdateCorporateAccountCommandValidator()
    {
        RuleFor(u => u.Id)
           .NotNull()
           .NotEmpty();
        RuleFor(u => u.Email)
            .NotNull()
            .NotEmpty();
        RuleFor(u => u.PhoneCode)
            .NotNull()
            .NotEmpty();
        RuleFor(u => u.PhoneNumber)
            .NotNull()
            .NotEmpty();
        RuleFor(u => u.PostalCode)
            .NotNull()
            .NotEmpty();
        RuleFor(u => u.Address)
            .NotNull()
            .NotEmpty();
        RuleFor(u => u.Country)
            .NotNull()
            .NotEmpty();
        RuleFor(u => u.CountryName)
            .NotNull()
            .NotEmpty();
        RuleFor(u => u.City)
            .NotNull()
            .NotEmpty();
        RuleFor(u => u.CityName)
            .NotNull()
            .NotEmpty();
        RuleFor(u => u.District)
            .NotNull()
            .NotEmpty();
        RuleFor(u => u.DistrictName)
            .NotNull()
            .NotEmpty();
    }
}
