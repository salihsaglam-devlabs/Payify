using FluentValidation;
using LinkPara.Emoney.Domain.Enums;

namespace LinkPara.Emoney.Application.Features.CompanyPools.Commands.SaveCompanyPool;

public class SaveCompanyPoolCommandValidator : AbstractValidator<SaveCompanyPoolCommand>
{
    public SaveCompanyPoolCommandValidator()
    {
        RuleFor(u => u.Title)
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
        RuleFor(u => u.TaxAdministration)
            .NotNull()
            .NotEmpty();
        RuleFor(u => u.TaxNumber)
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
        RuleFor(u => u.AuthorizedPersonIdentityNumber)
            .NotNull()
            .NotEmpty();
        RuleFor(u => u.AuthorizedPersonName)
            .NotNull()
            .NotEmpty();
        RuleFor(u => u.AuthorizedPersonSurname)
            .NotNull()
            .NotEmpty();
        RuleFor(u => u.AuthorizedPersonBirthDate)
            .NotNull()
            .NotEmpty();
        RuleFor(u => u.AuthorizedPersonCompanyPhoneCode)
            .NotNull()
            .NotEmpty();
        RuleFor(u => u.AuthorizedPersonCompanyPhoneNumber)
            .NotNull()
            .NotEmpty();
    }
}