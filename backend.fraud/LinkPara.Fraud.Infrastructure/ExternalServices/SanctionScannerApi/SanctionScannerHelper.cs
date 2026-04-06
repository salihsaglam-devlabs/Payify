using System.Text;

namespace LinkPara.Fraud.Infrastructure.ExternalServices.SanctionScannerApi;

public static class SanctionScannerHelper
{
    public static string GetBasicAuthorizationKey(string username, string password)
    {
        var plainTextBytes = Encoding.UTF8.GetBytes(username + ":" + password);
        return Convert.ToBase64String(plainTextBytes);
    }
}
