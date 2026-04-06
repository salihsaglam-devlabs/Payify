using FluentValidation;

namespace LinkPara.PF.Application.Features.PhysicalPos.Reconciliation.Queries.DownloadPhysicalPosEndOfDayDetailById;

public class DownloadPhysicalPosEndOfDayDetailByIdQueryValidator : AbstractValidator<DownloadPhysicalPosEndOfDayDetailByIdQuery>
{
    public DownloadPhysicalPosEndOfDayDetailByIdQueryValidator()
    {
        RuleFor(x => x.Id).NotNull().NotEmpty();
    }
}
