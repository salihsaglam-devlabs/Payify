using FluentValidation;

namespace LinkPara.Emoney.Application.Features.Limits.Queries.GetTierLevelById
{
    public class GetTierLevelByIdQueryValidator : AbstractValidator<GetTierLevelByIdQuery>
    {
        public GetTierLevelByIdQueryValidator()
        {
            RuleFor(x => x.Id).NotNull().NotEmpty();
        }
    }
}
