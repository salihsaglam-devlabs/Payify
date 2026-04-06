using FluentValidation;

namespace LinkPara.Fraud.Application.Features.Transactions.Commands.ExecuteTransactions;

public class ExecuteTransactionCommandValidator : AbstractValidator<ExecuteTransactionCommand>
{
    public ExecuteTransactionCommandValidator()
    {
        RuleFor(x => x.FraudCheckRequest.ExecuteTransaction.Amount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.FraudCheckRequest.ExecuteTransaction.AmountCurrencyCode).GreaterThanOrEqualTo(0);
        RuleFor(x => x.FraudCheckRequest.ExecuteTransaction.BeneficiaryAccountCurrencyCode).GreaterThanOrEqualTo(0);
        RuleFor(x => x.FraudCheckRequest.ExecuteTransaction.BeneficiaryNumber).NotNull().NotEmpty();
        RuleFor(x => x.FraudCheckRequest.ExecuteTransaction.OriginatorAccountCurrencyCode).GreaterThanOrEqualTo(0);
        RuleFor(x => x.FraudCheckRequest.ExecuteTransaction.OriginatorNumber).NotNull().NotEmpty();
        RuleFor(x => x.FraudCheckRequest.ExecuteTransaction.FraudSource).IsInEnum();
        RuleFor(x => x.FraudCheckRequest.ExecuteTransaction.Direction).IsInEnum();
    }
}
