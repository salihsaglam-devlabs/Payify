using FluentValidation;
using LinkPara.PF.Domain.Enums;

namespace LinkPara.PF.Application.Features.MerchantPools.Command.SaveMerchantPool;

public class SaveMerchantPoolCommandValidator : AbstractValidator<SaveMerchantPoolCommand>
{
    public SaveMerchantPoolCommandValidator()
    {
        RuleFor(x => x.MerchantName).NotNull().NotEmpty();
        RuleFor(x => x.CompanyType).IsInEnum();
        RuleFor(x => x.MerchantType).IsInEnum();
        RuleFor(x => x.CommercialTitle).NotNull().NotEmpty();
        RuleFor(x => x.WebSiteUrl).NotNull().NotEmpty()
                                  .WithMessage("Invalid web site url!");
        RuleFor(x => x.MonthlyTurnover).NotNull().NotEmpty();
        RuleFor(x => x.PostalCode).NotNull().NotEmpty()
                                  .Matches(@"[0-9]+$").Length(5);
        RuleFor(x => x.Address).NotNull().NotEmpty();
        RuleFor(x => x.PhoneCode).NotNull().NotEmpty();
        RuleFor(x => x.Country).NotNull().NotEmpty();
        RuleFor(x => x.CountryName).NotNull().NotEmpty();
        RuleFor(x => x.City).NotNull().NotEmpty();
        RuleFor(x => x.CityName).NotNull().NotEmpty();
        RuleFor(x => x.District).NotNull().NotEmpty();
        RuleFor(x => x.DistrictName).NotNull().NotEmpty();
        RuleFor(x => x.TaxAdministration).NotNull().NotEmpty();
        RuleFor(x => x.MoneyTransferStartHour)
            .InclusiveBetween(0, 23)
            .When(x => x.MoneyTransferStartHour.HasValue)
            .WithMessage("Hour must be between 0 and 23");
            
        RuleFor(x => x.MoneyTransferStartMinute)
            .InclusiveBetween(0, 59)
            .When(x => x.MoneyTransferStartMinute.HasValue)
            .WithMessage("Minute must be between 0 and 59");

        RuleFor(x => x.TradeRegistrationNumber).NotNull()
                                 .Matches(@"[0-9]+$").NotEmpty().Length(16);
        RuleFor(x => x.Iban)
            .Matches(@"[0-9]+$").Length(26)
            .When(x => !string.IsNullOrEmpty(x.Iban))
            .WithMessage("IBAN must be 26 digits when provided.");
        RuleFor(x => x.Email).EmailAddress().WithMessage("Invalid Email!");
        RuleFor(x => x.CompanyEmail).EmailAddress().WithMessage("Invalid Email!");

        When(b => b.CompanyType == CompanyType.Individual, () =>
        {
            RuleFor(x => x.AuthorizedPersonIdentityNumber).NotNull().NotEmpty()
                                .Matches(@"[0-9]+$").Length(11);
        });
        
        When(b => b.PostingPaymentChannel == PostingPaymentChannel.Wallet, () =>
        {
            RuleFor(x => x.WalletNumber).NotNull().NotEmpty().WithMessage("WalletNumber is required when payment channel is wallet.");
        });

        When(b => b.CompanyType != CompanyType.Individual, () =>
        {
            RuleFor(x => x.TaxNumber).NotNull().NotEmpty()
                                     .Matches(@"[0-9]+$").MinimumLength(10).MaximumLength(11);
        });
        RuleFor(x => x.AuthorizedPersonName).NotNull().NotEmpty();
        RuleFor(x => x.AuthorizedPersonSurname).NotNull().NotEmpty();
        RuleFor(x => x.AuthorizedPersonBirthDate).NotNull().NotEmpty();
        RuleFor(x => x.AuthorizedPersonCompanyPhoneNumber).NotNull().NotEmpty()
           .Must(x => x!.Trim().Length == 10)
           .WithMessage("Invalid PhoneNumber format");
        RuleFor(x => x.AuthorizedPersonMobilePhoneNumber).NotNull().NotEmpty()
           .Must(x => x!.Trim().Length == 10)
           .WithMessage("Invalid PhoneNumber format");
    }
}
