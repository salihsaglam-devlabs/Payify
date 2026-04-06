namespace LinkPara.Emoney.Application.Commons.Helpers;

public static class MaskedCardNumber
{
    public static string GetMaskedCardNumber(string cardNumber)
    {
        return cardNumber.Length < 14
            ? $"{cardNumber[..6]}{cardNumber[^(cardNumber.Length % 10)..].PadLeft(cardNumber.Length - 6, '*')}"
            : $"{cardNumber[..6]}{cardNumber[^4..].PadLeft(cardNumber.Length - 6, '*')}";
    }
}
