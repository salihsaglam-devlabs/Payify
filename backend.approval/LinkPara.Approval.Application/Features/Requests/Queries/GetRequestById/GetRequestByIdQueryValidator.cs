using FluentValidation;

namespace LinkPara.Approval.Application.Features.Requests.Queries.GetRequestById;

public class GetRequestByIdQueryValidator : AbstractValidator<GetRequestByIdQuery>
{
    public GetRequestByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotNull()
            .NotEmpty();
    }
}