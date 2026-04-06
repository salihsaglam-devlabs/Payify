using FluentValidation;

namespace LinkPara.Epin.Application.Features.Statistics.Queries;

public class GetReportQueryValidator : AbstractValidator<GetReportQuery>
{
	public GetReportQueryValidator()
	{
		RuleFor(s => s.StartDate)
			.NotNull()
			.NotEqual(DateTime.MinValue);

        RuleFor(s => s.EndDate)
            .NotNull()
            .NotEqual(DateTime.MinValue);
    }
}
