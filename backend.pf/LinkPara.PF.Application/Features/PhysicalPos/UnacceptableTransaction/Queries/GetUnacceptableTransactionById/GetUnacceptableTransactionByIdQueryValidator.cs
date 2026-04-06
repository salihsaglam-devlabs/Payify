using FluentValidation;

namespace LinkPara.PF.Application.Features.PhysicalPos.UnacceptableTransaction.Queries.GetUnacceptableTransactionById;

public class GetUnacceptableTransactionByIdQueryValidator : AbstractValidator<GetUnacceptableTransactionByIdQuery>
{
    public GetUnacceptableTransactionByIdQueryValidator()
    {
        RuleFor(x => x.Id).NotNull().NotEmpty();
    }
}
