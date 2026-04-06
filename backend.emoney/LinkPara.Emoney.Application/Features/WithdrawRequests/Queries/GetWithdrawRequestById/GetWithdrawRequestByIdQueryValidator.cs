using FluentValidation;

namespace LinkPara.Emoney.Application.Features.WithdrawRequests.Queries.GetWithdrawRequestById;

public class GetWithdrawRequestByIdQueryValidator : AbstractValidator<GetWithdrawRequestByIdQuery>
{
    public GetWithdrawRequestByIdQueryValidator()
    {
        RuleFor(s => s.Id)
            .NotNull()
            .NotEmpty();
    }
}