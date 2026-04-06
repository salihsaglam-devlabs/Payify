using System.Reflection;

namespace LinkPara.PF.Application.Commons.Models.PhysicalPos.Constants;

public static class PaxPinEntryInfo
{
    public const string OnlinePin = "ONLINE_PIN";
    public const string OfflinePin = "OFFLINE_PIN";
    public const string None = "NONE";
    
    public static string GetName(string value)
    {
        return typeof(PaxPinEntryInfo)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .FirstOrDefault(f => f.IsLiteral && !f.IsInitOnly && (string)f.GetValue(null)! == value)
            ?.Name;
    }
}