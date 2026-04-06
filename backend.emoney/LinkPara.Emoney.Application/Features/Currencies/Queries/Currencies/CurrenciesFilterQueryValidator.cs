using FluentValidation;
using LinkPara.Emoney.Domain.Enums;

namespace LinkPara.Emoney.Application.Features.Currencies.Queries.Currencies;

public class CurrenciesFilterQueryValidator : AbstractValidator<CurrenciesFilterQuery>
{
    public CurrenciesFilterQueryValidator()
    {
        RuleFor(s => s.Code)
            .Must(s => IsAllLetters(s))
            .Length(3)
            .When(s => !string.IsNullOrEmpty(s.Code));

        RuleFor(s => s.CurrencyType)
            .IsInEnum();
    }

    public static bool IsAllLetters(string s)
    {
        foreach (char c in s)
        {
            if (!Char.IsLetter(c))
                return false;
        }
        return true;
    }
}
