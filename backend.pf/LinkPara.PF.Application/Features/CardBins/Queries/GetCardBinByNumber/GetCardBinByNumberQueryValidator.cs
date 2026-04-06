using FluentValidation;

namespace LinkPara.PF.Application.Features.CardBins.Queries.GetCardBinByNumber;

public class GetCardBinByNumberQueryValidator : AbstractValidator<GetCardBinByNumberQuery>
{
    public GetCardBinByNumberQueryValidator()
    {
        RuleFor(x => x.BinNumber)
            .NotNull()
            .NotEmpty()
            .Length(6);
    }
}