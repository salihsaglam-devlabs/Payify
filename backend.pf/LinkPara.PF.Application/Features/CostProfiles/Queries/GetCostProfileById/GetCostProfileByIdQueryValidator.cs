using FluentValidation;

namespace LinkPara.PF.Application.Features.CostProfiles.Queries.GetCostProfileById;

public class GetCostProfileByIdQueryValidator: AbstractValidator<GetCostProfileByIdQuery>
{
	public GetCostProfileByIdQueryValidator()
	{
        RuleFor(s => s.Id).NotNull().NotEmpty();
    }
}
