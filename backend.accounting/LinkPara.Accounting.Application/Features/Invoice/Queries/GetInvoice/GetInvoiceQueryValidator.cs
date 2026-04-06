using FluentValidation;

namespace LinkPara.Accounting.Application.Features.Invoice.Queries.GetInvoice;

public class GetInvoiceQueryValidator : AbstractValidator<GetInvoiceQuery>
{
    public GetInvoiceQueryValidator()
    {
        RuleFor(p => p.PaymentClientReferenceId).NotEmpty().NotNull().NotEqual(Guid.Empty);
    }
}