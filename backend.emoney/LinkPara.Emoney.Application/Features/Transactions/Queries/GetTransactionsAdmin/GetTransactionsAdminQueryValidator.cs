using FluentValidation;

namespace LinkPara.Emoney.Application.Features.Transactions.Queries.GetTransactionsAdmin;

public class GetTransactionsAdminQueryValidator : AbstractValidator<GetTransactionsAdminQuery>
{
    public GetTransactionsAdminQueryValidator()
    {
    }
}