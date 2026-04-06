using FluentValidation;
using LinkPara.PF.Application.Commons.Models.Merchants;
using LinkPara.PF.Domain.Enums;

namespace LinkPara.PF.Application.Features.Boa.Merchants.Command.CreateBoaMerchant;

public class CreateBoaMerchantCommandValidator : AbstractValidator<CreateBoaMerchantCommand>
{
    public CreateBoaMerchantCommandValidator()
    {
        RuleFor(x => x.MerchantName)
            .NotNull()
            .NotEmpty()
            .MaximumLength(150)
            .WithMessage("MerchantName is required and maximum 150 characters allowed");
        
        RuleFor(x => x.MerchantType)
            .IsInEnum()
            .WithMessage("Invalid MerchantType");
        
        When(b => b.MerchantType == MerchantType.SubMerchant, () =>
        {
            RuleFor(x => x.ParentMerchantId)
                .NotNull()
                .NotEmpty()
                .WithMessage("ParentMerchantId is required when MerchantType is SubMerchant");
        });
        
        RuleFor(x => x.MoneyTransferStartHour)
            .InclusiveBetween(0, 23)
            .When(x => x.MoneyTransferStartHour.HasValue)
            .WithMessage("Hour must be between 0 and 23");
            
        RuleFor(x => x.MoneyTransferStartMinute)
            .InclusiveBetween(0, 59)
            .When(x => x.MoneyTransferStartMinute.HasValue)
            .WithMessage("Minute must be between 0 and 59");
        
        RuleFor(x => x.Customer).SetValidator(new CreateMerchantCustomerValidator());
        
        RuleFor(x => x.TechnicalContact).SetValidator(new CreateMerchantContactPersonValidator());
        
        RuleFor(x => x.AdminUser).SetValidator(new CreateMerchantUserValidator());
        
        RuleFor(x => x.WebSiteUrl)
            .NotNull()
            .NotEmpty()
            .WithMessage("Invalid web site url")
            .MaximumLength(150)
            .Matches(@"^(https?:\/\/)?(www\.)?([a-zA-Z0-9-]+\.)+[a-zA-Z]{2,}$")
            .WithMessage("WebSiteUrl must be a valid domain or URL and maximum 150 characters allowed");
        
        RuleFor(x => x.Language)
            .NotNull()
            .NotEmpty()
            .Must(lang => lang is "EN" or "TR")
            .WithMessage("Language must be EN or TR");
        
        RuleFor(x => x.MonthlyTurnover)
            .NotNull()
            .NotEmpty()
            .Must(x => x >= 0)
            .WithMessage("MonthlyTurnover must be greater than or equal to 0");
        
        RuleFor(x => x.SalesPersonId)
            .Must(g => g == null || g != Guid.Empty)
            .WithMessage("SalesPersonId must be a valid GUID when provided");
        
        RuleFor(x => x.MccCode)
            .NotNull()
            .NotEmpty()
            .MaximumLength(4)
            .WithMessage("MccCode must be 4 characters long");
        
        RuleFor(x => x.HostingTaxNo)
            .NotNull()
            .NotEmpty()
            .Matches(@"[0-9]+$")
            .MinimumLength(10)
            .MaximumLength(11)
            .WithMessage("HostingTaxNo must be numeric and between 10 and 11 characters");
        
        RuleFor(x => x.HostingUrl)
            .NotNull()
            .NotEmpty()
            .WithMessage("Invalid HostingUrl")
            .MaximumLength(150)
            .Matches(@"^(https?:\/\/)?(www\.)?([a-zA-Z0-9-]+\.)+[a-zA-Z]{2,}$")
            .WithMessage("HostingUrl must be a valid domain or URL and maximum 150 characters allowed");
        
        RuleFor(x => x.HostingTradeName)
            .NotNull()
            .NotEmpty()
            .MaximumLength(150)
            .WithMessage("HostingTradeName is required and maximum 150 characters allowed");
        
        RuleFor(x => x.MerchantIntegratorId)
            .Must(g => g == null || g != Guid.Empty)
            .WithMessage("MerchantIntegratorId must be a valid GUID when provided");
        
        RuleFor(x => x.ApplicationChannel)
            .IsInEnum()
            .WithMessage("Invalid ApplicationChannel");
        
        RuleFor(x => x.AgreementDate)
            .NotNull()
            .NotEmpty()
            .WithMessage("Invalid AgreementDate");
        
        RuleFor(x => x.IntegrationMode)
            .Must(mode => mode != IntegrationMode.Unknown && HasValidFlags(mode))
            .WithMessage("IntegrationMode must be a valid combination of defined flags.");
        
        RuleFor(x => x.PhoneCode)
            .NotNull()
            .NotEmpty()
            .WithMessage("PhoneCode is required")
            .Matches(@"^\+\d{1,4}$")
            .WithMessage("PhoneCode must be in format like +90");
        
        RuleFor(x => x.PricingProfileNumber)
            .NotNull()
            .NotEmpty()
            .MaximumLength(6)
            .WithMessage("Maximum 6 characters allowed for PricingProfileNumber");
        
        RuleFor(x => x.PostingPaymentChannel)
            .IsInEnum()
            .Must(x => x is PostingPaymentChannel.Wallet or PostingPaymentChannel.BankAccount)
            .WithMessage("PostingPaymentChannel must be Wallet or BankAccount");
        
        When(b => b.PostingPaymentChannel == PostingPaymentChannel.Wallet, () =>
        {
            RuleFor(x => x.MerchantWalletNumber)
                .NotNull()
                .NotEmpty()
                .MaximumLength(26)
                .WithMessage("MerchantWalletNumber is required when PostingPaymentChannel is Wallet");
        });
        
        RuleFor(x => x.MerchantIbanBankCode)
            .NotNull()
            .NotEmpty()
            .WithMessage("Bank code cannot be empty");
        
        RuleFor(x => x.MerchantIban)
            .NotEmpty()
            .Matches(@"^TR\d{24}$")
            .WithMessage("Invalid MerchantIban format");
        
        // RuleFor(x => x.VposList)
        //     .Must((obj, vposList) =>
        //         obj.IsAllBanksEnabled == true || vposList != null && vposList.Any()
        //     )
        //     .WithMessage("At least one Vpos must be given when AllBanks is not enabled");
        
        RuleForEach(x => x.MerchantLimits).SetValidator(new CreateMerchantLimitValidator());
    }
    
    private bool HasValidFlags(IntegrationMode mode)
    {
        var allValid = Enum.GetValues(typeof(IntegrationMode))
            .Cast<IntegrationMode>()
            .Where(v => v != IntegrationMode.Unknown)
            .Aggregate((a, b) => a | b);

        return (mode & ~allValid) == 0;
    }
    
    private class CreateMerchantContactPersonValidator : AbstractValidator<CreateMerchantContactPerson>
    {
        public CreateMerchantContactPersonValidator()
        {
            RuleFor(x => x.IdentityNumber)
                .NotNull()
                .NotEmpty()
                .Length(11)
                .WithMessage("IdentityNumber must be 11 digits long");

            RuleFor(x => x.Name)
                .NotNull()
                .NotEmpty()
                .MaximumLength(100)
                .WithMessage("Name is required and maximum 100 characters allowed");

            RuleFor(x => x.Surname)
                .NotNull()
                .NotEmpty()
                .MaximumLength(100)
                .WithMessage("Surname is required and maximum 100 characters allowed");

            RuleFor(x => x.Email)
                .NotNull()
                .NotEmpty()
                .MaximumLength(256)
                .EmailAddress()
                .WithMessage("Invalid Email");
            
            When(b =>  !string.IsNullOrEmpty(b.CompanyEmail), () =>
            {
                RuleFor(x => x.CompanyEmail)
                    .MaximumLength(256)
                    .EmailAddress()
                    .WithMessage("Invalid CompanyEmail");
            });

            RuleFor(s => s.BirthDate)
                .NotNull()
                .NotEmpty()
                .WithMessage("Invalid Birth date");

            RuleFor(x => x.CompanyPhoneNumber)
                .NotNull()
                .NotEmpty()
                .Must(x => x!.Trim().Length == 10)
                .WithMessage("CompanyPhoneNumber is required and must be 10 digits long");
            
            RuleFor(x => x.MobilePhoneNumber)
                .NotNull()
                .NotEmpty()
                .Must(x => x!.Trim().Length == 10)
                .WithMessage("MobilePhoneNumber is required and must be 10 digits long");
            
            When(b =>  !string.IsNullOrEmpty(b.MobilePhoneNumberSecond), () =>
            {
                RuleFor(x => x.MobilePhoneNumberSecond)
                    .Must(x => x!.Trim().Length == 10)
                    .WithMessage("MobilePhoneNumberSecond must be 10 digits long");
            });
        }
    }

    private class CreateMerchantCustomerValidator : AbstractValidator<CreateMerchantCustomer>
    {
        public CreateMerchantCustomerValidator()
        {
            RuleFor(x => x.CompanyType)
                .IsInEnum()
                .WithMessage("Invalid CompanyType");
            
            When(b => b.CompanyType == CompanyType.Individual, () =>
            {
                RuleFor(x => x.AuthorizedPerson.IdentityNumber)
                    .NotNull()
                    .NotEmpty()
                    .MaximumLength(11)
                    .WithMessage("Invalid Authorized Person Identity Number");
            });

            RuleFor(x => x.CommercialTitle)
                .NotNull()
                .NotEmpty()
                .MaximumLength(100)
                .WithMessage("CommercialTitle is required and maximum 100 characters allowed");

            When(b =>  !string.IsNullOrEmpty(b.TradeRegistrationNumber), () =>
            {
                RuleFor(x => x.TradeRegistrationNumber)
                    .MaximumLength(16)
                    .Matches(@"[0-9]+$")
                    .WithMessage("Maximum 16 characters allowed for TradeRegistrationNumber and must be numeric");
            });
            
            RuleFor(x => x.TaxAdministration)
                .NotNull()
                .NotEmpty()
                .MaximumLength(200)
                .WithMessage("TaxAdministration is required and maximum 200 characters allowed");
            
            RuleFor(x => x.TaxNumber)
                .NotNull()
                .NotEmpty()
                .Matches(@"[0-9]+$")
                .MinimumLength(10)
                .MaximumLength(11)
                .WithMessage("TaxNumber must be numeric and between 10 and 11 characters");
            
            RuleFor(x => x.Country)
                .NotNull()
                .NotEmpty()
                .WithMessage("Country is required");
            
            RuleFor(x => x.CountryName)
                .NotNull()
                .NotEmpty()
                .MaximumLength(200)
                .WithMessage("CountryName is required and maximum 200 characters allowed");
            
            RuleFor(x => x.City)
                .NotNull()
                .NotEmpty()
                .WithMessage("City is required");
            
            RuleFor(x => x.CityName)
                .NotNull()
                .NotEmpty()
                .MaximumLength(200)
                .WithMessage("CityName is required and maximum 200 characters allowed");
            
            RuleFor(x => x.District)
                .NotNull()
                .NotEmpty()
                .WithMessage("District is required");
            
            RuleFor(x => x.DistrictName)
                .NotNull()
                .NotEmpty()
                .MaximumLength(200)
                .WithMessage("DistrictName is required and maximum 200 characters allowed");
            
            RuleFor(x => x.PostalCode)
                .NotNull()
                .NotEmpty()
                .Matches(@"[0-9]+$")
                .Length(5)
                .WithMessage("PostalCode must be numeric and 5 digits long");
            
            RuleFor(x => x.Address)
                .NotNull()
                .NotEmpty()
                .MaximumLength(256)
                .WithMessage("Address is required and maximum 256 characters allowed");
            
            RuleFor(x => x.AuthorizedPerson)
                .SetValidator(new CreateMerchantContactPersonValidator());
        }
    }
    
    private class CreateMerchantUserValidator : AbstractValidator<CreateMerchantUser>
    {
        public CreateMerchantUserValidator()
        {
            RuleFor(x => x.Name)
                .NotNull()
                .NotEmpty()
                .MaximumLength(100)
                .WithMessage("Name is required and maximum 100 characters allowed");

            RuleFor(x => x.Surname)
                .NotNull()
                .NotEmpty()
                .MaximumLength(100)
                .WithMessage("Surname is required and maximum 100 characters allowed");

            RuleFor(x => x.Email)
                .NotNull()
                .NotEmpty()
                .MaximumLength(256)
                .EmailAddress()
                .WithMessage("Invalid Email");
            
            RuleFor(x => x.MobilePhoneNumber)
                .NotNull()
                .NotEmpty()
                .Must(x => x!.Trim().Length == 10)
                .WithMessage("MobilePhoneNumber is required and must be 10 digits long");
            
            RuleFor(x => x.PhoneCode)
                .NotNull()
                .NotEmpty()
                .WithMessage("PhoneCode is required")
                .Matches(@"^\+\d{1,4}$")
                .WithMessage("PhoneCode must be in format like +90");
            
            RuleFor(s => s.BirthDate)
                .NotNull()
                .NotEmpty()
                .WithMessage("Invalid Birth date");
            
            RuleFor(s => s.RoleId)
                .NotNull()
                .NotEmpty()
                .WithMessage("Invalid RoleId");
        }
    }
    
    private class CreateMerchantLimitValidator : AbstractValidator<CreateMerchantLimit>
    {
        public CreateMerchantLimitValidator()
        {
            RuleFor(x => x.TransactionLimitType)
                .IsInEnum()
                .WithMessage("Invalid TransactionLimitType");

            RuleFor(x => x.Period)
                .IsInEnum()
                .WithMessage("Invalid Period");

            RuleFor(x => x.LimitType)
                .IsInEnum()
                .WithMessage("Invalid LimitType");

            When(b => b.LimitType == LimitType.Amount, () =>
            {
                RuleFor(b => b.MaxAmount)
                    .NotNull()
                    .NotEmpty()
                    .WithMessage("MaxAmount is required when LimitType is Amount");
                
                RuleFor(b => b.CurrencyCode)
                    .NotNull()
                    .NotEmpty()
                    .WithMessage("CurrencyCode is required when LimitType is Amount");
            });

            When(b => b.LimitType == LimitType.Count, () =>
            {
                RuleFor(b => b.MaxPiece)
                    .NotNull()
                    .NotEmpty()
                    .WithMessage("MaxPiece is required when LimitType is Count");
            });
        }
    }
}