using FluentValidation;

namespace LinkPara.PF.Application.Features.CardBins.Queries.GetCardBinById;

public class GetCardBinByIdQueryValidator : AbstractValidator<GetCardBinByIdQuery>
{
    public GetCardBinByIdQueryValidator()
    {
        RuleFor(x => x.Id)
       .NotNull().NotEmpty();
    }
}
