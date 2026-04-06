using FluentValidation;

namespace LinkPara.Billing.Application.Features.Billing.Queries.GetBillTransactions;

public class GetBillTransactionsQueryValidator : AbstractValidator<GetBillTransactionsQuery>
{
    public GetBillTransactionsQueryValidator()
    {
        RuleFor(s => s.VendorId).NotEqual(Guid.Empty);
        RuleFor(s => s.SectorId).NotEqual(Guid.Empty);
        RuleFor(s => s.InstitutionId).NotEqual(Guid.Empty);
        RuleFor(s => s.BillDueStartDate).NotEqual(DateTime.MinValue);
        RuleFor(s => s.BillDueEndDate).NotEqual(DateTime.MinValue);
        RuleFor(s => s.PaymentStartDate).NotEqual(DateTime.MinValue);
        RuleFor(s => s.PaymentEndDate).NotEqual(DateTime.MinValue);
    }
}