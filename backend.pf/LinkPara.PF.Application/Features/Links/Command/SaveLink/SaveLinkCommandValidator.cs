using FluentValidation;
using LinkPara.PF.Domain.Enums;

namespace LinkPara.PF.Application.Features.Links.Command.SaveLink;

public class SaveLinkCommandValidator : AbstractValidator<SaveLinkCommand>
{
    public SaveLinkCommandValidator()
    {
        RuleFor(b => b.MerchantId)
            .NotNull()
            .NotEmpty();

        RuleFor(b => b.Currency)
         .NotNull()
         .NotEmpty();

        RuleFor(b => b.ExpiryDate)
         .NotNull()
         .NotEmpty();

        RuleFor(b => b.OrderId)
         .NotNull()
         .NotEmpty()
         .MaximumLength(24);

        RuleFor(x => x.ProductName)
            .NotEmpty()
            .NotNull()
            .MaximumLength(50);

        RuleFor(x => x.ProductDescription)
            .NotEmpty()
            .NotNull()
            .MaximumLength(100);

        When(x => x.LinkAmountType == LinkAmountType.FixedAmount, () =>
        {
            RuleFor(x => x.Amount).NotNull().NotEmpty();
        });

        When(x => x.LinkType == LinkType.MultipleUse, () =>
        {
            RuleFor(x => x.MaxUsageCount)
            .NotNull()
            .NotEmpty()
            .InclusiveBetween(2, 99999)
            .WithMessage("Kullanım sayısı değeri 2 ile 99999 arasında olmalıdır.");
        });
    }
}