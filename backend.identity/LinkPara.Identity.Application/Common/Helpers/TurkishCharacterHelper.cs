namespace LinkPara.Identity.Application.Common.Helpers;

public static class TurkishCharacterHelper
{
    private static readonly char[] TurkishCharacters = { 'ğ', 'Ğ', 'ü', 'Ü', 'ş', 'Ş', 'ı', 'İ', 'ö', 'Ö', 'ç', 'Ç' };

    public static bool ContainsTurkishCharacters(string input)
        => input.Any(x => TurkishCharacters.Contains(x));
}