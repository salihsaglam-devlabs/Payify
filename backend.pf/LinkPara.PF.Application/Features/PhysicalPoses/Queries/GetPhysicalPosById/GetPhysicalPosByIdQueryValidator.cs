using FluentValidation;

namespace LinkPara.PF.Application.Features.PhysicalPoses.Queries.GetPhysicalPosById;

public class GetPhysicalPosByIdQueryValidator : AbstractValidator<GetPhysicalPosByIdQuery>
{
    public GetPhysicalPosByIdQueryValidator()
    {
        RuleFor(s => s.Id).NotNull().NotEmpty();
    }
}
