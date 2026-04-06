using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkPara.Epin.Application.Features.Reconciliations.Queries.GetFilterReconciliationSummaries;

public class GetFilterReconciliationSummariesQueryValidator : AbstractValidator<GetFilterReconciliationSummariesQuery>
{
    public GetFilterReconciliationSummariesQueryValidator()
    {
        RuleFor(b => b.ReconciliationDateEnd)
            .GreaterThan(b => b.ReconciliationDateStart.Value)
            .WithMessage("End date must after Start date!")
            .When(b => b.ReconciliationDateStart.HasValue);
    }
}
