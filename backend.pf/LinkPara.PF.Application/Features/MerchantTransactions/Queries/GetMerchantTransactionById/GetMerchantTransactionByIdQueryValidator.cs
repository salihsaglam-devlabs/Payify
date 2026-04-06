using FluentValidation;

namespace LinkPara.PF.Application.Features.MerchantTransactions.Queries.GetMerchantTransactionById;

public class GetMerchantTransactionByIdQueryValidator : AbstractValidator<GetMerchantTransactionByIdQuery>
{
    public GetMerchantTransactionByIdQueryValidator()
    {
        RuleFor(x => x.Id)
       .NotNull().NotEmpty();
    }
}
