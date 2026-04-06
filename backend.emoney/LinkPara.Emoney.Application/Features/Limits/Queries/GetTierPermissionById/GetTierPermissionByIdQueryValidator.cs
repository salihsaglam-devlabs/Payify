using FluentValidation;

namespace LinkPara.Emoney.Application.Features.Limits.Queries.GetTierPermissionById;

public class GetTierPermissionByIdQueryValidator : AbstractValidator<GetTierPermissionByIdQuery>
{
    public GetTierPermissionByIdQueryValidator()
    {
        RuleFor(x => x.Id).NotNull().NotEmpty();
    }
}