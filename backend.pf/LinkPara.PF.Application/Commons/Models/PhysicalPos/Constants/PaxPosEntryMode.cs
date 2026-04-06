using System.Reflection;

namespace LinkPara.PF.Application.Commons.Models.PhysicalPos.Constants;
public static class PaxPosEntryMode
{
    public const string ManualEntry = "01";
    public const string QrOperations = "03";
    public const string ContactEmv = "05";
    public const string ContactlessEmv = "07";
    public const string EmvTechnicalFallback = "80";
    public const string MagStripe = "90";
    public const string ContactlessMagStripe = "91";
    
    public static string GetName(string value)
    {
        return typeof(PaxPosEntryMode)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .FirstOrDefault(f => f.IsLiteral && !f.IsInitOnly && (string)f.GetValue(null)! == value)
            ?.Name;
    }
}