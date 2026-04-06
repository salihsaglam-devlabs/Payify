using System.Security;

namespace LinkPara.Security.Helpers;

public static class StringHelper
{
    public static String ToPlainString(this SecureString secureStr)
    {
        String plainStr = new System.Net.NetworkCredential(string.Empty,
                          secureStr).Password;
        return plainStr;
    }

    public static SecureString ToSecureString(this String plainStr)
    {
        var secStr = new SecureString(); secStr.Clear();
        foreach (char c in plainStr.ToCharArray())
        {
            secStr.AppendChar(c);
        }
        return secStr;
    }
}