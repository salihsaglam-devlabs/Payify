using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkPara.Identity.Application.Features.Account.Queries.CheckUserInformation
{
    public class CheckUserForRegisterQueryValidator : AbstractValidator<CheckUserForRegisterQuery>
    {
        public CheckUserForRegisterQueryValidator() {
            RuleFor(x => x.Msisdn)
             .NotNull().WithMessage("MSISDN cannot be null.")
             .NotEmpty().WithMessage("MSISDN cannot be empty.");

            RuleFor(x => x.Tckn)
                .NotNull().WithMessage("TCKN cannot be null.")
                .NotEmpty().WithMessage("TCKN cannot be empty.");

            RuleFor(x => x.Name)
                .NotNull().WithMessage("Name cannot be null.")
                .NotEmpty().WithMessage("Name cannot be empty.");

            RuleFor(x => x.Surname)
                .NotNull().WithMessage("Surname cannot be null.")
                .NotEmpty().WithMessage("Surname cannot be empty.");

            RuleFor(x => x.Nationality)
                .NotNull().WithMessage("Nationality cannot be null.")
                .NotEmpty().WithMessage("Nationality cannot be empty.");

            RuleFor(x => x.BirthDate)
                .NotNull().WithMessage("BirthDate cannot be null.")
                .LessThan(DateTime.Now).WithMessage("BirthDate must be in the past.");

            RuleFor(x => x.Email)
                .NotNull().WithMessage("Email cannot be null.")
                .NotEmpty().WithMessage("Email cannot be empty.")
                .MaximumLength(256)
                .EmailAddress(FluentValidation.Validators.EmailValidationMode.AspNetCoreCompatible).WithMessage("Email must be a valid email address.");

            RuleFor(x => x.Documents)
                .NotNull().WithMessage("Documents cannot be null.")
                .NotEmpty().WithMessage("Documents cannot be empty.");

            RuleForEach(x => x.Documents).ChildRules(documents =>
            {
                documents.RuleFor(d => d.Id)
                    .NotNull().WithMessage("Document ID cannot be null.")
                    .NotEmpty().WithMessage("Document ID cannot be empty.");

                documents.RuleFor(d => d.IsAccepted)
                    .NotNull().WithMessage("Document acceptance status cannot be null.");
            });
        }
    }
}
