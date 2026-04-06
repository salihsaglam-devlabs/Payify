using FluentValidation;

namespace LinkPara.Billing.Application.Features.Vendors.Commands;

public class SaveVendorCommandValidator : AbstractValidator<SaveVendorCommand>
{
    public SaveVendorCommandValidator()
    {
        RuleFor(s => s.Name).NotEmpty().NotNull();
        RuleFor(s => s.RecordStatus).NotNull();
    }
}