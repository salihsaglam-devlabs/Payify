using LinkPara.Emoney.Application.Commons.Models;
using System.Text;

namespace LinkPara.Emoney.Application.Commons.Helpers;

public static class AmountTextConverter
{
    public static string DecimalToWords(decimal amount)
    {
        amount = Math.Round(amount, 2, MidpointRounding.AwayFromZero);

        if (amount < 0)
            return "Eksi " + DecimalToWords(-amount);

        long wholePart = (long)amount;
        int fractionalPart = (int)((amount - wholePart) * 100);

        string wholePartText = NumberToWords(wholePart);
        string fractionalPartText = fractionalPart > 0 ? $" {NumberToWords(fractionalPart)} Kuruş" : "";

        return wholePartText + " Türk Lirası" + fractionalPartText;
    }

    private static string NumberToWords(long number)
    {
        if (number == 0)
            return "Sıfır";

        string[] thousands = { "", "Bin", "Milyon", "Milyar", "Trilyon" };

        string words = "";
        int groupIndex = 0;

        while (number > 0)
        {
            int groupValue = (int)(number % 1000);

            if (groupValue > 0)
            {
                string groupText = ConvertGroupToWords(groupValue);
                if (groupIndex > 0 && groupValue == 1 && groupIndex == 1)
                    groupText = thousands[groupIndex]; 
                else
                    groupText += " " + thousands[groupIndex];

                var word = new StringBuilder();
                word.Append(groupText);
                word.Append(" ");
                word.Append(words);

                words = word.ToString();
            }

            number /= 1000;
            groupIndex++;
        }

        return words.Trim();
    }

    private static string ConvertGroupToWords(int number)
    {
        string[] ones = { "", "Bir", "İki", "Üç", "Dört", "Beş", "Altı", "Yedi", "Sekiz", "Dokuz" };
        string[] tens = { "", "On", "Yirmi", "Otuz", "Kırk", "Elli", "Altmış", "Yetmiş", "Seksen", "Doksan" };

        string result = "";

        if (number >= 100)
        {
            int hundreds = number / 100;
            result += (hundreds > 1 ? ones[hundreds] + " Yüz" : "Yüz") + " ";
            number %= 100;
        }

        if (number >= 10)
        {
            int tensPlace = number / 10;
            result += tens[tensPlace] + " ";
            number %= 10;
        }

        if (number > 0)
        {
            result += ones[number] + " ";
        }

        return result.Trim();
    }
}
