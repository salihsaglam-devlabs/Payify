using FluentValidation;

namespace LinkPara.Billing.Application.Features.Institutions.Commands;

public class UpdateInstitutionCommandValidator : AbstractValidator<UpdateInstitutionCommand>
{
    public UpdateInstitutionCommandValidator()
    {
        RuleFor(s => s.InstitutionId).NotEmpty().NotNull();
        RuleFor(s => s.ActiveVendorId).NotEmpty().NotNull();
        RuleFor(s => s.RecordStatus).NotNull();
    }
}
