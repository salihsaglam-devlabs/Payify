using FluentValidation;
using LinkPara.PF.Application.Commons.Models.Customers;
using LinkPara.PF.Application.Commons.Models.Merchants;
using LinkPara.PF.Domain.Enums;

namespace LinkPara.PF.Application.Features.Merchants.Command.UpdateMerchant;

public class UpdateMerchantCommandValidator : AbstractValidator<UpdateMerchantCommand>
{
    public UpdateMerchantCommandValidator()
    {
        When(b => b.MerchantStatus != MerchantStatus.Draft, () =>
        {
            RuleFor(x => x.Name).MaximumLength(150)
          .WithMessage("Invalid Merchant name!");

            RuleFor(x => x.Language).MaximumLength(100)
             .WithMessage("Invalid Language!");

            RuleFor(x => x.PricingProfileNumber).NotNull().NotEmpty();

            RuleFor(x => x.MccCode).NotNull().NotEmpty();
            
            RuleFor(x => x.NaceCode).NotNull().NotEmpty();

            RuleFor(x => x.ApplicationChannel).IsInEnum();

            RuleFor(x => x.PostingPaymentChannel).IsInEnum();

            RuleFor(x => x.PhoneCode).NotNull().NotEmpty();

            RuleFor(x => x.WebSiteUrl).NotNull().NotEmpty()
                                    .WithMessage("Invalid web site url!");

            RuleFor(x => x.MonthlyTurnover).NotNull().NotEmpty();

            RuleFor(x => x.MoneyTransferStartHour)
                .InclusiveBetween(0, 23)
                .When(x => x.MoneyTransferStartHour.HasValue)
                .WithMessage("Hour must be between 0 and 23");
            
            RuleFor(x => x.MoneyTransferStartMinute)
                .InclusiveBetween(0, 59)
                .When(x => x.MoneyTransferStartMinute.HasValue)
                .WithMessage("Minute must be between 0 and 59");

            When(x => !x.IsPaymentToMainMerchant, () =>
            {
                RuleFor(x => x.MerchantBankAccounts)
                    .NotNull()
                    .Must(x => x.Any())
                    .WithMessage("Merchant bank accounts must be provided when payment is not made to the main merchant.");
            });

            When(x => x.PosType == PosType.Virtual, () =>
            {
                RuleFor(x => x.HostingTaxNo).MinimumLength(10).MaximumLength(11)
             .WithMessage("Invalid Tecnical Person Hosting Tax No!");

                RuleFor(x => x.TechnicalContact)
             .SetValidator(new SaveContactPersonValidator());

            });

            RuleForEach(x => x.MerchantBankAccounts)
             .SetValidator(new SaveMerchantBankAccountValidator());

            RuleForEach(x => x.MerchantDocuments)
             .SetValidator(new SaveMerchantDocumentValidator());

            RuleForEach(x => x.MerchantLimits)
           .SetValidator(new SaveMerchantLimitValidator());

            RuleFor(x => x.Customer)
          .SetValidator(new UpdateCustomerValidator());
        });
    }
}

public class SaveContactPersonValidator : AbstractValidator<MerchantContactPersonDto>
{
    public SaveContactPersonValidator()
    {
        RuleFor(x => x.IdentityNumber).MaximumLength(11)
          .WithMessage("Invalid Tecnical Person Identity Number!");

        RuleFor(x => x.Name).MaximumLength(100)
          .WithMessage("Invalid Tecnical Person Name!");

        RuleFor(x => x.Surname).MaximumLength(100)
          .WithMessage("Invalid Tecnical Person Surname!");

        RuleFor(x => x.Email).MaximumLength(256)
           .EmailAddress().WithMessage("Invalid Tecnical Person Email!");

        RuleFor(s => s.BirthDate)
           .NotNull()
           .NotEmpty()
           .WithMessage("Invalid Tecnical Person Birth date!");

        RuleFor(x => x.CompanyPhoneNumber).NotNull().NotEmpty()
           .Must(x => x!.Trim().Length == 10)
           .WithMessage("Invalid Tecnical Person PhoneNumber format");
        RuleFor(x => x.MobilePhoneNumber).NotNull().NotEmpty()
           .Must(x => x!.Trim().Length == 10)
           .WithMessage("Invalid Tecnical Person PhoneNumber format");
        RuleFor(x => x.MobilePhoneNumber).NotNull().NotEmpty()
           .Must(x => x!.Trim().Length == 10)
           .WithMessage("Invalid Tecnical Person PhoneNumber format");
    }
}

public class SaveMerchantVposValidator : AbstractValidator<SaveMerchantVposRequest>
{
    public SaveMerchantVposValidator()
    {
        RuleFor(x => x.Priority).NotNull().NotEmpty();

        RuleFor(x => x.VposId).NotNull().NotEmpty();
    }
}

public class UpdateCustomerValidator : AbstractValidator<CustomerDto>
{
    public UpdateCustomerValidator()
    {
        RuleFor(x => x.CompanyType).IsInEnum();

        When(b => b.CompanyType == CompanyType.Individual, () =>
        {
            RuleFor(x => x.AuthorizedPerson.IdentityNumber).MaximumLength(11)
               .WithMessage("Invalid Authorized Person Identity Number!");
        });

        RuleFor(x => x.CommercialTitle).NotNull().NotEmpty();

        RuleFor(x => x.PostalCode).NotNull().NotEmpty()
                                  .Matches(@"[0-9]+$").Length(5);
        RuleFor(x => x.Address).NotNull().NotEmpty();

        RuleFor(x => x.Country).NotNull().NotEmpty();
        RuleFor(x => x.CountryName).NotNull().NotEmpty();
        RuleFor(x => x.City).NotNull().NotEmpty();
        RuleFor(x => x.CityName).NotNull().NotEmpty();
        RuleFor(x => x.District).NotNull().NotEmpty();
        RuleFor(x => x.DistrictName).NotNull().NotEmpty();
        RuleFor(x => x.TaxAdministration).NotNull().NotEmpty();
        RuleFor(x => x.TaxNumber).NotNull().NotEmpty()
                                 .Matches(@"[0-9]+$").MinimumLength(10).MaximumLength(11);
        RuleFor(x => x.TradeRegistrationNumber).NotNull()
                                 .Matches(@"[0-9]+$").NotEmpty().Length(16);

        RuleFor(x => x.AuthorizedPerson)
      .SetValidator(new UpdateAuthorizedPersonValidator());
    }
}

public class UpdateAuthorizedPersonValidator : AbstractValidator<MerchantContactPersonDto>
{
    public UpdateAuthorizedPersonValidator()
    {
        RuleFor(x => x.Name).MaximumLength(100)
          .WithMessage("Invalid Authorized Person Name!");

        RuleFor(x => x.Surname).MaximumLength(100)
          .WithMessage("Invalid Authorized Person Surname!");

        RuleFor(x => x.Email).MaximumLength(256)
           .EmailAddress().WithMessage("Invalid Authorized Person Email!");

        RuleFor(x => x.CompanyEmail).MaximumLength(256)
          .EmailAddress().WithMessage("Invalid Company Email!");

        RuleFor(s => s.BirthDate)
           .NotNull()
           .NotEmpty()
           .WithMessage("Invalid Authorized Person Birth date!");

        RuleFor(x => x.CompanyPhoneNumber).NotNull().NotEmpty()
           .Must(x => x!.Trim().Length == 10)
           .WithMessage("Invalid Authorized Person PhoneNumber format");
        RuleFor(x => x.MobilePhoneNumber).NotNull().NotEmpty()
           .Must(x => x!.Trim().Length == 10)
           .WithMessage("Invalid Authorized Person PhoneNumber format");
        RuleFor(x => x.MobilePhoneNumber).NotNull().NotEmpty()
           .Must(x => x!.Trim().Length == 10)
           .WithMessage("Invalid Authorized Person PhoneNumber format");
    }
}

public class SaveMerchantBankAccountValidator : AbstractValidator<MerchantBankAccountDto>
{
    public SaveMerchantBankAccountValidator()
    {
        RuleFor(x => x.Iban).MaximumLength(32)
            .When(x => !string.IsNullOrEmpty(x.Iban))
          .WithMessage("Invalid Iban!");
    }
}

public class SaveMerchantDocumentValidator : AbstractValidator<MerchantDocumentDto>
{
    public SaveMerchantDocumentValidator()
    {
        RuleFor(x => x.DocumentId).NotNull().NotEmpty();

        RuleFor(x => x.DocumentTypeId).NotNull().NotEmpty();

        RuleFor(x => x.DocumentName).NotNull().NotEmpty();
    }
}

public class SaveMerchantLimitValidator : AbstractValidator<MerchantLimitDto>
{
    public SaveMerchantLimitValidator()
    {
        RuleFor(x => x.TransactionLimitType).IsInEnum();

        RuleFor(x => x.Period).IsInEnum();

        RuleFor(x => x.LimitType).IsInEnum();

        When(b => b.LimitType == LimitType.Amount, () =>
        {
            RuleFor(b => b.MaxAmount).NotNull().NotEmpty()
            .WithMessage("Amount not null!");
        });

        When(b => b.LimitType == LimitType.Count, () =>
        {
            RuleFor(b => b.MaxPiece).NotNull().NotEmpty()
            .WithMessage("Count not null!");
        });
    }
}


