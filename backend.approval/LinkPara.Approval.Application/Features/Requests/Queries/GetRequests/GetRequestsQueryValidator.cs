using FluentValidation;

namespace LinkPara.Approval.Application.Features.Requests.Queries.GetRequests;

public class GetRequestsQueryValidator : AbstractValidator<GetRequestsQuery>
{
    public GetRequestsQueryValidator()
    {
        RuleFor(s => s.UserRoleIds)
            .NotNull()
            .NotEmpty();
    }
}
