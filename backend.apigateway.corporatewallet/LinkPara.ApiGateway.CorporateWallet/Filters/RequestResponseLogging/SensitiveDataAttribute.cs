namespace LinkPara.ApiGateway.CorporateWallet.Filters.RequestResponseLogging;

[AttributeUsage(AttributeTargets.Method)]
public class SensitiveDataAttribute : Attribute
{
    public SensitiveDataType SensitiveData { get; set; }

    public SensitiveDataAttribute(SensitiveDataType type)
    {
        SensitiveData = type;
    }
}
