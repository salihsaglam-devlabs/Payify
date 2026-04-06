namespace LinkPara.Card.Application.Commons.Helpers;

public static class HexHelper
{
    public static byte[] HexToBytes(string hex)
    {
        hex = hex.Replace(" ", string.Empty);

        if (hex.Length % 2 != 0)
            throw new ArgumentException("Hex string length must be even.");

        var bytes = new byte[hex.Length / 2];

        for (int i = 0; i < hex.Length; i += 2)
        {
            bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
        }

        return bytes;
    }

    public static string BytesToHex(byte[] bytes)
    {
        return BitConverter.ToString(bytes).Replace("-", string.Empty);
    }
}
