using FluentValidation;

namespace LinkPara.PF.Application.Features.PhysicalPos.Reconciliation.Queries.GetPhysicalPosEndOfDayDetailById;

public class GetPhysicalPosEndOfDayDetailByIdQueryValidator : AbstractValidator<GetPhysicalPosEndOfDayDetailByIdQuery>
{
    public GetPhysicalPosEndOfDayDetailByIdQueryValidator()
    {
        RuleFor(x => x.Id).NotNull().NotEmpty();
    }
}
