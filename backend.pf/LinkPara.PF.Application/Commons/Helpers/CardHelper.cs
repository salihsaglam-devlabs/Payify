using LinkPara.PF.Application.Features.CardBins;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;

namespace LinkPara.PF.Application.Commons.Helpers;

public static class CardHelper
{
    public static CardBrand GetCardBrand(string cardNumber)
    {
        if (string.IsNullOrEmpty(cardNumber)) return CardBrand.MasterCard;
        return cardNumber[..1] switch
        {
            "3" => CardBrand.Amex,
            "4" => CardBrand.Visa,
            "5" or "6" => CardBrand.MasterCard,
            _ => CardBrand.MasterCard,
        };
    }

    public static string GetMaskedCardNumber(string cardNumber)
    {
        return cardNumber.Length < 14
            ? $"{cardNumber[..6]}{cardNumber[^(cardNumber.Length % 10)..].PadLeft(cardNumber.Length - 6, '*')}"
            : $"{cardNumber[..6]}{cardNumber[^4..].PadLeft(cardNumber.Length - 6, '*')}";
    }

    public static CardTransactionType GetCardTransactionType(AcquireBank acquireBank, CardBinDto bin, List<CardLoyaltyException> cardLoyaltyExceptions)
    {
        if (acquireBank.BankCode == bin?.BankCode)
            return CardTransactionType.OnUs;

        if (bin?.CardType == CardType.Credit && acquireBank.CardNetwork == bin.CardNetwork && cardLoyaltyExceptions.All(s => s.BankCode != acquireBank.BankCode))
            return CardTransactionType.OnUs;
       

        return CardTransactionType.NotOnUs;
    }

    public static string GetBinNumber(string cardNumber)
    {
        return cardNumber[..6];
    }
}
