using FluentValidation;

namespace LinkPara.PF.Application.Features.VirtualPos.Queries.GetVposById;

public class GetVposByIdQueryValidator : AbstractValidator<GetVposByIdQuery>
{
    public GetVposByIdQueryValidator()
    {
        RuleFor(s => s.Id).NotNull().NotEmpty();
    }
}
