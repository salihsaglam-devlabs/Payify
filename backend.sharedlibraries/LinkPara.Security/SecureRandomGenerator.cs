using System.Security.Cryptography;

namespace LinkPara.Security;

public class SecureRandomGenerator : ISecureRandomGenerator
{
    public long GenerateSecureRandomNumber(int maxLength)
    {
        byte[] randomNumber = new byte[8];
        using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomNumber);
        }
        long scaledRandom = BitConverter.ToInt64(randomNumber, 0);
        scaledRandom = Math.Abs(scaledRandom);
        scaledRandom %= (long)Math.Pow(10, maxLength);
        while (scaledRandom.ToString().Length < maxLength)
        {
            byte[] randomByte = new byte[1];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomByte);
            }
            int lastDigit = randomByte[0] % 10;
            scaledRandom = (scaledRandom * 10) + lastDigit;
        }
        return scaledRandom;
    }

    public long GenerateSecureRandomNumber(long min, long max)
    {
        if (min >= max)
            throw new ArgumentException("Min value must be less than max value.");

        long range = max - min;

        using RandomNumberGenerator rng = RandomNumberGenerator.Create();
        var randomNumber = new byte[8];
        rng.GetBytes(randomNumber);

        var scaledRandom = Math.Abs(BitConverter.ToInt64(randomNumber, 0)) % range;

        scaledRandom += min;

        return scaledRandom;
    }
}
