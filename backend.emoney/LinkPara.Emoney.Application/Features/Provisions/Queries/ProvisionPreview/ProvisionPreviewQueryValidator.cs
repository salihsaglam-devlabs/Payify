using FluentValidation;
using System.Security.Cryptography.X509Certificates;

namespace LinkPara.Emoney.Application.Features.Provisions.Queries.ProvisionPreview
{
    public class ProvisionPreviewQueryValidator : AbstractValidator<ProvisionPreviewQuery>
    {
        public ProvisionPreviewQueryValidator()
        {
            RuleFor(c => c.WalletNumber)
            .MaximumLength(50)
            .NotEmpty()
            .NotNull();

            RuleFor(c => c.CurrencyCode)
                .MaximumLength(10)
                .NotEmpty()
                .NotNull();

            RuleFor(x => x.Amount)
               .NotEmpty()
               .NotNull()
               .Must(amount => amount > 0);
        }
    }
}
