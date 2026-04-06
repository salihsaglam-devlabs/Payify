using FluentValidation;

namespace LinkPara.Identity.Application.Features.Address.Queries.GetAddressByUserId;

public class GetAddressByUserIdQueryValidator : AbstractValidator<GetAddressByUserIdQuery>
{
    public GetAddressByUserIdQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotNull().NotEmpty();
    }
}